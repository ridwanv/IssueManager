// Browser & Audio Notifications for Agent Dashboard
window.agentNotifications = {
    // Browser notification support
    isNotificationSupported: function () {
        return 'Notification' in window;
    },

    getNotificationPermission: function () {
        if (!this.isNotificationSupported()) return 'unsupported';
        return Notification.permission;
    },

    requestNotificationPermission: async function () {
        if (!this.isNotificationSupported()) return 'unsupported';
        
        try {
            const permission = await Notification.requestPermission();
            return permission;
        } catch (error) {
            console.error('Error requesting notification permission:', error);
            return 'denied';
        }
    },

    showBrowserNotification: function (title, options = {}) {
        if (!this.isNotificationSupported() || Notification.permission !== 'granted') {
            return false;
        }

        try {
            const notification = new Notification(title, {
                icon: '/icon-192.png',
                badge: '/favicon.ico',
                requireInteraction: options.requireInteraction || false,
                tag: options.tag || 'agent-notification',
                ...options
            });

            // Auto-close notification after specified duration
            if (options.duration && options.duration > 0) {
                setTimeout(() => {
                    notification.close();
                }, options.duration);
            }

            // Handle notification click
            notification.onclick = function(event) {
                event.preventDefault();
                window.focus();
                if (options.clickCallback) {
                    options.clickCallback();
                }
                notification.close();
            };

            return true;
        } catch (error) {
            console.error('Error showing browser notification:', error);
            return false;
        }
    },

    // Audio notification support
    audioContext: null,
    audioBuffers: {},

    initializeAudio: async function () {
        try {
            // Create audio context with user gesture handling
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
            
            // Resume audio context if needed (required by some browsers)
            if (this.audioContext.state === 'suspended') {
                await this.audioContext.resume();
            }

            return true;
        } catch (error) {
            console.error('Error initializing audio:', error);
            return false;
        }
    },

    loadAudioBuffer: async function (url, name) {
        if (!this.audioContext) {
            await this.initializeAudio();
        }

        try {
            const response = await fetch(url);
            const arrayBuffer = await response.arrayBuffer();
            const audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
            this.audioBuffers[name] = audioBuffer;
            return true;
        } catch (error) {
            console.error(`Error loading audio ${name}:`, error);
            return false;
        }
    },

    playAudioNotification: function (soundName, volume = 1.0) {
        if (!this.audioContext || !this.audioBuffers[soundName]) {
            console.warn(`Audio ${soundName} not loaded or audio context not initialized`);
            return false;
        }

        try {
            const source = this.audioContext.createBufferSource();
            const gainNode = this.audioContext.createGain();
            
            source.buffer = this.audioBuffers[soundName];
            gainNode.gain.value = Math.max(0, Math.min(1, volume));
            
            source.connect(gainNode);
            gainNode.connect(this.audioContext.destination);
            
            source.start(0);
            return true;
        } catch (error) {
            console.error(`Error playing audio ${soundName}:`, error);
            return false;
        }
    },

    // Combined notification methods
    showEscalationNotification: function (customerPhone, reason, priority = 1, preferences = {}) {
        const title = `🚨 New Escalation`;
        const body = `Customer: ${customerPhone}\nReason: ${reason}`;
        const isPriority = priority > 1;
        
        const options = {
            body: body,
            icon: '/icon-192.png',
            requireInteraction: isPriority,
            tag: 'escalation',
            duration: isPriority ? 0 : 8000, // Critical notifications stay until clicked
            clickCallback: () => {
                // Focus on the dashboard when notification is clicked
                if (window.location.pathname !== '/agent-dashboard') {
                    window.location.href = '/agent-dashboard';
                }
            }
        };

        // Show browser notification if enabled in preferences
        if (preferences.browserNotifications !== false) {
            this.showBrowserNotification(title, options);
        }

        // Play audio notification if enabled in preferences
        if (preferences.audioNotifications !== false) {
            const soundName = isPriority ? 'priority-alert' : 'standard-alert';
            this.playAudioNotification(soundName, preferences.audioVolume || 0.7);
        }
    },

    showAgentStatusNotification: function (agentName, status, preferences = {}) {
        if (preferences.agentStatusNotifications === false) return;

        const title = `Agent Status Update`;
        const body = `${agentName} is now ${status}`;
        
        const options = {
            body: body,
            icon: '/icon-192.png',
            tag: 'agent-status',
            duration: 4000
        };

        this.showBrowserNotification(title, options);
    },

    showConversationAssignedNotification: function (agentName, preferences = {}) {
        if (preferences.assignmentNotifications === false) return;

        const title = `Conversation Assigned`;
        const body = `Conversation assigned to ${agentName}`;
        
        const options = {
            body: body,
            icon: '/icon-192.png',
            tag: 'conversation-assigned',
            duration: 3000
        };

        this.showBrowserNotification(title, options);
    },

    // Utility methods
    checkBrowserSupport: function () {
        return {
            notifications: this.isNotificationSupported(),
            audio: !!(window.AudioContext || window.webkitAudioContext),
            serviceWorker: 'serviceWorker' in navigator
        };
    },

    // Initialize default audio files
    initializeDefaultSounds: async function () {
        // Create simple notification sounds using Web Audio API if audio files are not available
        await this.createNotificationSounds();
    },

    createNotificationSounds: async function () {
        if (!this.audioContext) {
            await this.initializeAudio();
        }

        // Create simple beep sounds for notifications
        this.createBeepSound('standard-alert', 800, 0.3, 0.2); // 800Hz, 0.3s duration, 0.2 volume
        this.createBeepSound('priority-alert', 1200, 0.5, 0.5); // 1200Hz, 0.5s duration, 0.5 volume
    },

    createBeepSound: function (name, frequency, duration, volume) {
        if (!this.audioContext) return;

        const sampleRate = this.audioContext.sampleRate;
        const numSamples = Math.floor(sampleRate * duration);
        const buffer = this.audioContext.createBuffer(1, numSamples, sampleRate);
        const channelData = buffer.getChannelData(0);

        // Generate a simple sine wave with fade out
        for (let i = 0; i < numSamples; i++) {
            const t = i / sampleRate;
            const fadeOut = Math.max(0, 1 - (i / numSamples));
            channelData[i] = Math.sin(2 * Math.PI * frequency * t) * volume * fadeOut;
        }

        this.audioBuffers[name] = buffer;
    }
};

// Initialize audio context on first user interaction
document.addEventListener('click', function initAudioOnFirstClick() {
    window.agentNotifications.initializeDefaultSounds();
    document.removeEventListener('click', initAudioOnFirstClick);
}, { once: true });