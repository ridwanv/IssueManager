// Escalation Toast Mobile Gesture Support
window.escalationToastGestures = {
    // Touch tracking variables
    touchStartX: null,
    touchStartY: null,
    touchElement: null,
    isDragging: false,
    minSwipeDistance: 100,
    maxVerticalDeviation: 50,

    // Initialize swipe gesture support for a toast element
    initializeSwipeGestures: function(toastElement, dotNetRef) {
        if (!toastElement || !dotNetRef) {
            console.warn('Invalid parameters for toast swipe initialization');
            return;
        }

        // Add swipeable class
        toastElement.classList.add('escalation-toast-swipeable');

        // Store dotnet reference for later use
        toastElement._dotNetRef = dotNetRef;

        // Touch event listeners
        toastElement.addEventListener('touchstart', this.handleTouchStart.bind(this), { passive: false });
        toastElement.addEventListener('touchmove', this.handleTouchMove.bind(this), { passive: false });
        toastElement.addEventListener('touchend', this.handleTouchEnd.bind(this), { passive: false });
    },

    // Remove gesture support from toast element
    removeSwipeGestures: function(toastElement) {
        if (!toastElement) return;

        toastElement.classList.remove('escalation-toast-swipeable');
        toastElement.removeEventListener('touchstart', this.handleTouchStart);
        toastElement.removeEventListener('touchmove', this.handleTouchMove);
        toastElement.removeEventListener('touchend', this.handleTouchEnd);
        
        // Clean up stored reference
        delete toastElement._dotNetRef;
    },

    // Handle touch start event
    handleTouchStart: function(e) {
        if (e.touches.length !== 1) return;

        const touch = e.touches[0];
        this.touchStartX = touch.clientX;
        this.touchStartY = touch.clientY;
        this.touchElement = e.currentTarget;
        this.isDragging = false;

        // Add visual feedback class
        this.touchElement.style.transition = 'none';
    },

    // Handle touch move event
    handleTouchMove: function(e) {
        if (!this.touchStartX || !this.touchStartY || !this.touchElement) return;
        if (e.touches.length !== 1) return;

        const touch = e.touches[0];
        const deltaX = touch.clientX - this.touchStartX;
        const deltaY = Math.abs(touch.clientY - this.touchStartY);

        // Check if movement is more horizontal than vertical
        if (Math.abs(deltaX) > 10 && deltaY < this.maxVerticalDeviation) {
            this.isDragging = true;
            
            // Prevent page scrolling during swipe
            e.preventDefault();

            // Apply visual feedback - move toast with finger
            if (deltaX > 0) { // Swiping right
                const transformX = Math.min(deltaX, 150); // Limit maximum displacement
                this.touchElement.style.transform = `translateX(${transformX}px)`;
                
                // Show dismiss indicator when threshold is reached
                const indicator = this.touchElement.querySelector('.escalation-toast-swipe-indicator');
                if (indicator) {
                    if (deltaX > this.minSwipeDistance * 0.7) {
                        indicator.classList.add('visible');
                    } else {
                        indicator.classList.remove('visible');
                    }
                }
                
                // Add opacity feedback
                const opacity = Math.max(1 - (deltaX / 200), 0.3);
                this.touchElement.style.opacity = opacity;
            }
        }
    },

    // Handle touch end event
    handleTouchEnd: function(e) {
        if (!this.touchStartX || !this.touchElement) return;

        const deltaX = this.isDragging ? 
            (e.changedTouches[0].clientX - this.touchStartX) : 0;

        // Restore transition for smooth animation
        this.touchElement.style.transition = 'transform 0.3s ease-out, opacity 0.3s ease-out';

        // Check if swipe distance meets threshold for dismissal
        if (deltaX > this.minSwipeDistance) {
            // Dismiss the toast using .NET callback
            this.animateSwipeDismiss(this.touchElement);
        } else {
            // Reset toast position
            this.resetToastPosition(this.touchElement);
        }

        // Reset touch tracking
        this.touchStartX = null;
        this.touchStartY = null;
        this.touchElement = null;
        this.isDragging = false;
    },

    // Animate toast dismissal after successful swipe
    animateSwipeDismiss: function(toastElement) {
        toastElement.style.transform = 'translateX(100%)';
        toastElement.style.opacity = '0';

        // Hide swipe indicator
        const indicator = toastElement.querySelector('.escalation-toast-swipe-indicator');
        if (indicator) {
            indicator.classList.remove('visible');
        }

        // Call .NET dismiss callback after animation
        setTimeout(() => {
            const dotNetRef = toastElement._dotNetRef;
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnSwipeDismiss');
            }
        }, 300);
    },

    // Reset toast to original position
    resetToastPosition: function(toastElement) {
        toastElement.style.transform = 'translateX(0)';
        toastElement.style.opacity = '1';

        // Hide swipe indicator
        const indicator = toastElement.querySelector('.escalation-toast-swipe-indicator');
        if (indicator) {
            indicator.classList.remove('visible');
        }
    },

    // Check if device supports touch
    isTouchDevice: function() {
        return ('ontouchstart' in window) || 
               (navigator.maxTouchPoints > 0) || 
               (navigator.msMaxTouchPoints > 0);
    },

    // Get device orientation
    getOrientation: function() {
        if (screen.orientation && screen.orientation.angle !== undefined) {
            return screen.orientation.angle;
        }
        return window.orientation || 0;
    },

    // Debounce function for performance
    debounce: function(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
};

// Auto-initialize for mobile devices
if (window.escalationToastGestures.isTouchDevice()) {
    console.log('Touch device detected - escalation toast gestures available');
    
    // Listen for orientation changes
    window.addEventListener('orientationchange', window.escalationToastGestures.debounce(() => {
        console.log('Orientation changed:', window.escalationToastGestures.getOrientation());
        // Could trigger layout adjustments here if needed
    }, 250));
}
