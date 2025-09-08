// Simple test to verify browser console is working
export function testBrowserConsole(message) {
    console.log('[SIGNALR-TEST] ' + message);
    console.log('[SIGNALR-TEST] Browser console is working!');
    
    // Test if we can access the global SignalR connection
    if (window.blazorCulture) {
        console.log('[SIGNALR-TEST] Blazor culture detected:', window.blazorCulture);
    }
    
    return true;
}