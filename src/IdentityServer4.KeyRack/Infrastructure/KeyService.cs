using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using IdentityServer4.KeyRack.Model;

namespace IdentityServer4.KeyRack
{
    /// <summary>Default <see cref="IKeyService" /> implementation</summary>
    public class KeyService : IKeyService
    {
        private readonly SemaphoreSlim _rotationLock = new SemaphoreSlim(1);
        private readonly KeyRackOptions _options;
        private readonly IKeyRepository _repository;
        private readonly IKeySerializer _serializer;
        private readonly ITimeKeeper _timeKeeper;
        private readonly ILogger<KeyService> _logger;

        public KeyService(
            KeyRackOptions options,
            IKeyRepository repository,
            IKeySerializer serializer,
            ITimeKeeper timeKeeper,
            ILogger<KeyService> logger)
        {
            _options = options;
            _repository = repository;
            _serializer = serializer;
            _timeKeeper = timeKeeper;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RsaKey>> GetAllKeysAsync()
        {
            var processTuple = await ProcessKeysAsync();
            return processTuple.Item1;
        }

        /// <inheritdoc />
        public async Task<RsaKey> GetCurrentKeyAsync()
        {
            var processTuple = await ProcessKeysAsync();
            var key = processTuple.Item2;
            if (_logger.IsEnabled(LogLevel.Information))
            {
                var keyAge = _timeKeeper.GetKeyAge(key);
                _logger.LogInformation(
                    "Found active key id = {keyId}. Key will expire after {expiration}. Key will retire after {retirement}.",
                    key.KeyId,
                    _options.KeyExpiration.Subtract(keyAge),
                    _options.KeyRetirement.Subtract(keyAge));
            }

            return key;
        }

        private async Task<(IEnumerable<RsaKey>, RsaKey)> ProcessKeysAsync()
        {
            var keys = await LoadKeysAsync();
            var activeKey = GetActiveKey(keys);
            var rotationDue = false;

            if (activeKey != null)
            {
                rotationDue = IsRotationDue(keys);
            }

            if (activeKey == null || rotationDue)
            {
                _logger.LogDebug("Entering key rotation lock.");
                await _rotationLock.WaitAsync();
                try
                {
                    keys = await LoadKeysAsync();
                    activeKey ??= GetActiveKey(keys);
                    if (rotationDue)
                    {
                        rotationDue = IsRotationDue(keys);
                    }

                    if (activeKey == null || rotationDue)
                    {
                        if (activeKey == null)
                        {
                            _logger.LogDebug("No active key found - will create new key.");
                        }
                        else
                        {
                            _logger.LogDebug("Key rotation required due to approaching retirement - will create new key");
                        }

                        (keys, activeKey) = await RotateKeys();
                    }
                    else
                    {
                        _logger.LogDebug("Other server created new key.");
                    }
                }
                finally
                {
                    _logger.LogDebug("Releasing key rotation lock.");
                    _rotationLock.Release();
                }
            }

            return (keys, activeKey);
        }

        private RsaKey GetActiveKey(IEnumerable<RsaKey> keys)
        {
            if (keys == null || !keys.Any())
            {
                return null;
            }

            var activeKey = GetActiveKeyUseDelay(keys);
            if (activeKey == null)
            {
                activeKey = GetActiveKeyUseDelay(keys, useActivationDelay: false);
            }

            return activeKey;
        }

        private RsaKey GetActiveKeyUseDelay(IEnumerable<RsaKey> keys, bool useActivationDelay = true)
        {
            if (keys == null)
            {
                return null;
            }

            var activeKeys = keys.Where(key => _timeKeeper.IsActive(key, useActivationDelay)).ToArray();
            return activeKeys.OrderBy(key => key.Created).FirstOrDefault();
        }

        private async Task<IEnumerable<RsaKey>> LoadKeysAsync()
        {
            var storedKeys = await _repository.LoadKeysAsync();
            IEnumerable<RsaKey> keys = storedKeys.Select(_serializer.Deserialize).Where(x => x != null).ToArray();
            return await DeleteRetiredKeysAsync(keys);
        }

        private async Task<IEnumerable<RsaKey>> DeleteRetiredKeysAsync(IEnumerable<RsaKey> keys)
        {
            var retiredKeys = keys.Where(_timeKeeper.IsRetired).ToArray();
            foreach (var retiredKey in retiredKeys)
            {
                await _repository.DeleteKeyAsync(retiredKey.KeyId);
            }

            return keys.Except(retiredKeys).ToArray();
        }

        private bool IsRotationDue(IEnumerable<RsaKey> keys)
        {
            if (keys == null || !keys.Any())
            {
                return true;
            }

            var activeKey = GetActiveKey(keys);
            if (activeKey == null)
            {
                return true;
            }

            var activationPendingKey =
                keys.Where(key => key.Created > activeKey.Created)
                    .OrderByDescending(key => key.Created)
                    .FirstOrDefault();
            activeKey = activationPendingKey ?? activeKey;
            return _options.KeyExpiration.Subtract(_timeKeeper.GetKeyAge(activeKey)) < _options.KeyActivation;
        }

        private async Task<(IEnumerable<RsaKey>, RsaKey)> RotateKeys()
        {
            await InsertNewKey();
            if (_options.KeyInitialization > TimeSpan.Zero)
            {
                await Task.Delay(_options.KeyInitialization);
            }

            var keys = await LoadKeysAsync();
            return (keys, GetActiveKey(keys));
        }

        private Task InsertNewKey()
        {
            var securityKey = _options.CreateSecurityKey();
            var now = _timeKeeper.UtcNow;
            var key = new RsaKey(securityKey, now);
            return _repository.StoreKeyAsync(_serializer.Serialize(key));
        }
    }
}