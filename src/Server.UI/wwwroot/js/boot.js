(() => {
    const maximumRetryCount = 10; // Much higher retry count
    const retryIntervalMilliseconds = 1000; // Very fast reconnection
    const reconnectModal = document.getElementById('reconnect-modal');
    
    // Add connection state tracking
    let connectionFailures = 0;
    const maxConnectionFailures = 20; // Allow more failures before giving up
    
    console.log('Boot.js: Initializing Blazor connection handler');

    const startReconnectionProcess = () => {
        reconnectModal.style.display = 'block';

        let isCanceled = false;

        (async () => {
            for (let i = 0; i < maximumRetryCount; i++) {
                const messageElement = document.getElementById('reconnect-message') || reconnectModal;
            messageElement.innerText = `Attempting to reconnect: ${i + 1} of ${maximumRetryCount}`;
            console.log(`Boot.js: Reconnection attempt ${i + 1} of ${maximumRetryCount}`);

                await new Promise(resolve => setTimeout(resolve, retryIntervalMilliseconds));

                if (isCanceled) {
                    return;
                }

                try {
                    const result = await Blazor.reconnect();
                    if (!result) {
                        // The server was reached, but the connection was rejected; reload the page.
                        connectionFailures++;
                        if (connectionFailures >= maxConnectionFailures) {
                            console.log('Too many connection failures. Forcing hard refresh...');
                            location.reload(true); // Hard reload
                        } else {
                            location.reload();
                        }
                        return;
                    }

                    // Successfully reconnected to the server.
                    connectionFailures = 0; // Reset failure count on success
                    return;
                } catch (error) {
                    // Didn't reach the server; try again.
                    console.log(`Reconnection attempt ${i + 1} failed:`, error);
                    connectionFailures++;
                }
            }

            // Retried too many times; reload the page.
            console.log('Maximum retry attempts reached. Forcing hard refresh...');
            location.reload(true); // Force hard reload to bypass cache
        })();

        return {
            cancel: () => {
                isCanceled = true;
                reconnectModal.style.display = 'none';
            },
        };
    };

    let currentReconnectionProcess = null;

    Blazor.start({
        reconnectionHandler: {
            onConnectionDown: () => currentReconnectionProcess ??= startReconnectionProcess(),
            onConnectionUp: () => {
                currentReconnectionProcess?.cancel();
                currentReconnectionProcess = null;
            },
        },
    });
})();