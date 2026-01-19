/**
 * Wishlist JavaScript Module
 * Handles all wishlist operations including add, remove, and display
 * Phase 3.3: Separate from cart to prevent conflict
 */

(function ($) {
    'use strict';

    // Wishlist module
    window.Wishlist = {
        /**
         * Initialize wishlist functionality
         */
        init: function () {
            this.bindEvents();
            this.updateWishlistCount();
        },

        /**
         * Bind event handlers
         */
        bindEvents: function () {
            const self = this;

            // Add to wishlist button
            // Uses .btn-add-to-wishlist for explicit wishlist actions
            $(document).on('click', '.btn-add-to-wishlist', function (e) {
                e.preventDefault();
                e.stopPropagation();
                
                const $btn = $(this);
                const productId = $btn.data('product-id');

                if (!productId) {
                    console.error('Product ID is required to add to wishlist');
                    return;
                }

                self.addToWishlist(productId, $btn);
            });

            // Legacy support for .wishlist class (if not already has .btn-add-to-wishlist)
            // This ensures backwards compatibility with existing markup
            $(document).on('click', '.wishlist:not(.btn-add-to-wishlist)', function (e) {
                e.preventDefault();
                e.stopPropagation();
                
                const $btn = $(this);
                
                // Get product ID from button or parent card
                let productId = $btn.data('product-id') || 
                               $btn.data('productId') ||
                               $btn.closest('.card-product, .product-item, [data-product-id]').data('product-id') ||
                               $btn.closest('.card-product, .product-item, [data-product-id]').data('productId');

                if (!productId) {
                    console.warn('Wishlist: Product ID not found');
                    return;
                }

                self.toggleWishlist(productId, $btn);
            });

            // Remove from wishlist
            $(document).on('click', '.remove-wishlist', function (e) {
                e.preventDefault();
                e.stopPropagation();
                
                const $btn = $(this);
                const productId = $btn.data('product-id');

                if (!productId) {
                    console.error('Product ID is required to remove from wishlist');
                    return;
                }

                self.removeFromWishlist(productId, $btn);
            });
        },

        /**
         * Add product to wishlist
         */
        addToWishlist: function (productId, $btn) {
            const self = this;
            
            // Toggle active state for visual feedback
            $btn.toggleClass('active');

            // Update wishlist count (simulated for now)
            // In production, this would make an AJAX call to the server
            const $countBox = $('.count-box').first();
            let count = parseInt($countBox.text()) || 0;
            
            if ($btn.hasClass('active')) {
                count++;
                self.showMessage('Product added to wishlist!', 'success');
            } else {
                count = Math.max(0, count - 1);
                self.showMessage('Product removed from wishlist', 'info');
            }
            
            self.updateWishlistCount(count);

            // TODO: In production, implement AJAX call to /wishlist/add endpoint
            // $.ajax({
            //     url: '/wishlist/add',
            //     type: 'POST',
            //     contentType: 'application/json',
            //     data: JSON.stringify({ productId: parseInt(productId) }),
            //     success: function(response) {
            //         if (response.success) {
            //             self.showMessage('Product added to wishlist!', 'success');
            //             self.updateWishlistCount(response.wishlistCount);
            //         }
            //     }
            // });
        },

        /**
         * Toggle wishlist item (for legacy .wishlist buttons)
         */
        toggleWishlist: function (productId, $btn) {
            // Simply toggle active class for now
            $btn.toggleClass('active');
            
            // Update count
            const $countBox = $('.count-box').first();
            let count = parseInt($countBox.text()) || 0;
            
            if ($btn.hasClass('active')) {
                count++;
            } else {
                count = Math.max(0, count - 1);
            }
            
            this.updateWishlistCount(count);
        },

        /**
         * Remove product from wishlist
         */
        removeFromWishlist: function (productId, $btn) {
            const self = this;
            const $row = $btn.closest('.wishlist-item, .product-item');

            // Remove the row with animation
            $row.fadeOut(300, function () {
                $(this).remove();

                // Check if wishlist is empty
                if ($('.wishlist-item, .product-item').length === 0) {
                    // Show empty wishlist message
                    $('.wishlist-container').html(`
                        <div class="empty-wishlist text-center py-5">
                            <i class="icon icon-heart" style="font-size: 64px; color: #ccc;"></i>
                            <p class="mt-3 mb-3">Your wishlist is empty</p>
                            <a href="/shop" class="tf-btn btn-fill animate-hover-btn radius-3">
                                <span>Continue Shopping</span>
                            </a>
                        </div>
                    `);
                }
            });

            self.showMessage('Item removed from wishlist', 'success');
            self.updateWishlistCount();
        },

        /**
         * Update wishlist count in header
         */
        updateWishlistCount: function (count) {
            if (count !== undefined) {
                // Use provided count - for wishlist icon specifically
                $('.nav-icon-item .count-box').first().text(count);
            }
            // If no count provided, you could fetch from server
            // For now, we just use the visual toggle
        },

        /**
         * Show message to user
         * Uses themed UI notifications if available, falls back to alert
         */
        showMessage: function (message, type) {
            // Use UI.notify if available (Ecomus themed notifications)
            if (window.UI && window.UI.notify) {
                switch (type) {
                    case 'success':
                        window.UI.notify.success(message);
                        break;
                    case 'error':
                        window.UI.notify.error(message);
                        break;
                    case 'warning':
                        window.UI.notify.warning(message);
                        break;
                    case 'info':
                        window.UI.notify.info(message);
                        break;
                    default:
                        window.UI.notify.info(message);
                }
            } else if (typeof toastr !== 'undefined') {
                // Fallback to toastr if available
                toastr[type](message);
            } else {
                // Last resort: console log (silent for wishlist toggles)
                console.log('[Wishlist]', message);
            }
        }
    };

    // Initialize wishlist when DOM is ready
    $(document).ready(function () {
        Wishlist.init();
    });

})(jQuery);
