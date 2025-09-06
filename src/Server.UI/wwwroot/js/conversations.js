// Conversation-related JavaScript utilities

window.scrollToBottom = (elementId) => {
    try {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollTop = element.scrollHeight;
            element.scrollIntoView({ behavior: 'smooth', block: 'end' });
        }
    } catch (error) {
        console.warn('Could not scroll to bottom:', error);
    }
};

window.autoResizeTextarea = (element) => {
    try {
        if (element) {
            element.style.height = 'auto';
            element.style.height = (element.scrollHeight) + 'px';
        }
    } catch (error) {
        console.warn('Could not auto-resize textarea:', error);
    }
};

window.focusElement = (elementId) => {
    try {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus();
        }
    } catch (error) {
        console.warn('Could not focus element:', error);
    }
};

// Play notification sound for escalation popups
window.playNotificationSound = (type) => {
    try {
        if (type === 'escalation') {
            // Create a simple notification sound
            const audioContext = new (window.AudioContext || window.webkitAudioContext)();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();
            
            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);
            
            oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
            oscillator.frequency.setValueAtTime(600, audioContext.currentTime + 0.1);
            oscillator.frequency.setValueAtTime(800, audioContext.currentTime + 0.2);
            
            gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.3);
            
            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 0.3);
        }
    } catch (error) {
        console.warn('Could not play notification sound:', error);
    }
};

// Auto-dismiss timeout function
window.setTimeout = (dotNetObject, methodName, timeout) => {
    return setTimeout(() => {
        try {
            dotNetObject.invokeMethodAsync(methodName);
        } catch (error) {
            console.warn('Could not invoke .NET method:', error);
        }
    }, timeout);
};

window.clearTimeout = (timeoutId) => {
    clearTimeout(timeoutId);
};