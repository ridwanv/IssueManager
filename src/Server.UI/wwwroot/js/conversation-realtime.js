// Conversation real-time functionality
window.conversationRealtime = {
    connection: null,
    connectionStatus: 'disconnected',
    
    // Initialize SignalR connection
    initConnection: function(hubUrl) {
        return new Promise((resolve, reject) => {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(hubUrl)
                .withAutomaticReconnect([0, 2000, 10000, 30000])
                .build();

            this.connection.onreconnecting((error) => {
                this.connectionStatus = 'reconnecting';
                this.updateConnectionStatus('reconnecting');
                console.log('SignalR connection reconnecting...', error);
            });

            this.connection.onreconnected((connectionId) => {
                this.connectionStatus = 'connected';
                this.updateConnectionStatus('connected');
                console.log('SignalR connection reconnected:', connectionId);
            });

            this.connection.onclose((error) => {
                this.connectionStatus = 'disconnected';
                this.updateConnectionStatus('disconnected');
                console.log('SignalR connection closed:', error);
            });

            this.connection.start()
                .then(() => {
                    this.connectionStatus = 'connected';
                    this.updateConnectionStatus('connected');
                    console.log('SignalR connection started');
                    resolve();
                })
                .catch(err => {
                    this.connectionStatus = 'error';
                    this.updateConnectionStatus('error');
                    console.error('SignalR connection error:', err);
                    reject(err);
                });
        });
    },

    // Update connection status indicator
    updateConnectionStatus: function(status) {
        const statusElement = document.querySelector('.connection-status');
        if (statusElement) {
            let text, color, icon;
            switch (status) {
                case 'connected':
                    text = 'Connected';
                    color = 'success';
                    icon = 'check_circle';
                    break;
                case 'reconnecting':
                    text = 'Reconnecting...';
                    color = 'warning';
                    icon = 'sync';
                    break;
                case 'disconnected':
                    text = 'Disconnected';
                    color = 'error';
                    icon = 'error';
                    break;
                case 'error':
                    text = 'Connection Error';
                    color = 'error';
                    icon = 'error';
                    break;
            }
            
            statusElement.textContent = text;
            statusElement.className = `connection-status mud-chip mud-chip-size-small mud-chip-color-${color}`;
            
            const iconElement = statusElement.querySelector('.mud-chip-icon');
            if (iconElement) {
                iconElement.textContent = icon;
            }
        }
    },

    // Join conversation group
    joinConversation: function(conversationId) {
        if (this.connection && this.connectionStatus === 'connected') {
            return this.connection.invoke('JoinConversationGroup', conversationId);
        }
        return Promise.reject('Connection not available');
    },

    // Leave conversation group
    leaveConversation: function(conversationId) {
        if (this.connection && this.connectionStatus === 'connected') {
            return this.connection.invoke('LeaveConversationGroup', conversationId);
        }
        return Promise.reject('Connection not available');
    },

    // Smooth scroll to bottom of messages
    scrollToBottom: function(containerId, smooth = true) {
        const container = document.getElementById(containerId);
        if (container) {
            const scrollOptions = {
                top: container.scrollHeight,
                behavior: smooth ? 'smooth' : 'auto'
            };
            container.scrollTo(scrollOptions);
        }
    },

    // Check if user is near bottom of message container
    isNearBottom: function(containerId, threshold = 100) {
        const container = document.getElementById(containerId);
        if (!container) return false;
        
        const scrollTop = container.scrollTop;
        const scrollHeight = container.scrollHeight;
        const clientHeight = container.clientHeight;
        
        return scrollTop + clientHeight >= scrollHeight - threshold;
    },

    // Update browser tab title with notification count
    updateTabTitle: function(originalTitle, unreadCount = 0) {
        if (unreadCount > 0) {
            document.title = `(${unreadCount}) ${originalTitle}`;
        } else {
            document.title = originalTitle;
        }
    },

    // Play notification sound
    playNotificationSound: function() {
        // Create and play a subtle notification sound
        const audioContext = window.AudioContext || window.webkitAudioContext;
        if (audioContext) {
            const context = new audioContext();
            const oscillator = context.createOscillator();
            const gainNode = context.createGain();
            
            oscillator.connect(gainNode);
            gainNode.connect(context.destination);
            
            oscillator.frequency.setValueAtTime(800, context.currentTime);
            oscillator.frequency.setValueAtTime(600, context.currentTime + 0.1);
            
            gainNode.gain.setValueAtTime(0, context.currentTime);
            gainNode.gain.linearRampToValueAtTime(0.1, context.currentTime + 0.01);
            gainNode.gain.exponentialRampToValueAtTime(0.01, context.currentTime + 0.3);
            
            oscillator.start(context.currentTime);
            oscillator.stop(context.currentTime + 0.3);
        }
    },

    // Add message with animation
    addMessageWithAnimation: function(messageElement) {
        if (messageElement) {
            messageElement.style.opacity = '0';
            messageElement.style.transform = 'translateY(20px)';
            
            // Trigger animation
            requestAnimationFrame(() => {
                messageElement.style.transition = 'all 0.3s ease-out';
                messageElement.style.opacity = '1';
                messageElement.style.transform = 'translateY(0)';
            });
        }
    },

    // Dispose connection
    dispose: function() {
        if (this.connection) {
            this.connection.stop();
            this.connection = null;
        }
    }
};

// Global scroll to bottom function (for backward compatibility)
window.scrollToBottom = function(containerId) {
    window.conversationRealtime.scrollToBottom(containerId);
};