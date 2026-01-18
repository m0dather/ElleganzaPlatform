/**
 * Shopping Cart JavaScript Module
 * Handles all cart operations including add, update, remove, and display
 */

(function ($) {
    'use strict';

    // Cart module
    window.Cart = {
        /**
         * Initialize cart functionality
         */
        init: function () {
            this.bindEvents();
            this.updateCartCount();
        },

        /**
         * Bind event handlers
         */
        bindEvents: function () {
            const self = this;

            // Add to cart button (on product pages)
            $(document).on('click', '.btn-add-to-cart', function (e) {
                e.preventDefault();
                const $btn = $(this);
                const productId = $btn.data('product-id');
                const quantity = $btn.closest('form, .product-form').find('input[name="number"]').val() || 1;

                if (!productId) {
                    console.error('Product ID is required to add to cart');
                    return;
                }

                self.addToCart(productId, quantity, $btn);
            });

            // Update quantity in cart page
            $(document).on('click', '.btn-quantity', function (e) {
                e.preventDefault();
                const $btn = $(this);
                const $input = $btn.siblings('input[type="text"]');
                const productId = $input.data('product-id');
                let quantity = parseInt($input.val()) || 1;

                if ($btn.hasClass('btnincrease') || $btn.hasClass('plus-btn')) {
                    quantity++;
                } else if ($btn.hasClass('btndecrease') || $btn.hasClass('minus-btn')) {
                    quantity = Math.max(1, quantity - 1);
                }

                $input.val(quantity);

                if (productId) {
                    self.updateCartItem(productId, quantity);
                }
            });

            // Update quantity on input change (cart page)
            $(document).on('change', '.wg-quantity input[type="text"]', function () {
                const $input = $(this);
                const productId = $input.data('product-id');
                const quantity = parseInt($input.val()) || 1;

                if (productId) {
                    self.updateCartItem(productId, quantity);
                }
            });

            // Remove item from cart
            $(document).on('click', '.remove-cart', function (e) {
                e.preventDefault();
                const $btn = $(this);
                const productId = $btn.data('product-id');

                if (!productId) {
                    console.error('Product ID is required to remove from cart');
                    return;
                }

                if (confirm('Are you sure you want to remove this item from cart?')) {
                    self.removeFromCart(productId, $btn);
                }
            });
        },

        /**
         * Add product to cart
         */
        addToCart: function (productId, quantity, $btn) {
            const self = this;
            const originalText = $btn.html();

            // Disable button and show loading state
            $btn.prop('disabled', true).html('<span>Adding...</span>');

            $.ajax({
                url: '/cart/add',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    productId: parseInt(productId),
                    quantity: parseInt(quantity)
                }),
                success: function (response) {
                    if (response.success) {
                        // Show success message
                        self.showMessage('Product added to cart successfully!', 'success');
                        
                        // Update cart count
                        self.updateCartCount(response.cartCount);

                        // Show mini cart modal if exists
                        const $modal = $('#shoppingCart');
                        if ($modal.length) {
                            $modal.modal('show');
                        }
                    } else {
                        self.showMessage(response.message || 'Failed to add product to cart', 'error');
                    }
                },
                error: function (xhr) {
                    let errorMessage = 'Failed to add product to cart';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    }
                    self.showMessage(errorMessage, 'error');
                },
                complete: function () {
                    // Re-enable button and restore text
                    $btn.prop('disabled', false).html(originalText);
                }
            });
        },

        /**
         * Update cart item quantity
         */
        updateCartItem: function (productId, quantity) {
            const self = this;

            $.ajax({
                url: '/cart/update',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    productId: parseInt(productId),
                    quantity: parseInt(quantity)
                }),
                success: function (response) {
                    if (response.success) {
                        // Update cart count
                        self.updateCartCount(response.cartCount);

                        // Update totals on the page
                        self.updateCartTotals(response);

                        // Update item total
                        const $row = $('[data-product-id="' + productId + '"]');
                        if ($row.length) {
                            const unitPrice = parseFloat($row.find('.tf-cart-item_price .price').text().replace('$', ''));
                            const itemTotal = unitPrice * quantity;
                            $row.find('.tf-cart-item_total .price').text('$' + itemTotal.toFixed(2));
                        }
                    } else {
                        self.showMessage(response.message || 'Failed to update cart', 'error');
                    }
                },
                error: function (xhr) {
                    let errorMessage = 'Failed to update cart';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    }
                    self.showMessage(errorMessage, 'error');
                }
            });
        },

        /**
         * Remove item from cart
         */
        removeFromCart: function (productId, $btn) {
            const self = this;
            const $row = $btn.closest('.tf-cart-item');

            $.ajax({
                url: '/cart/remove/' + productId,
                type: 'POST',
                success: function (response) {
                    if (response.success) {
                        // Remove the row with animation
                        $row.fadeOut(300, function () {
                            $(this).remove();

                            // Check if cart is empty
                            if ($('.tf-cart-item').length === 0) {
                                // Reload page to show empty cart message
                                window.location.reload();
                            } else {
                                // Update cart count and totals
                                self.updateCartCount(response.cartCount);
                                self.updateCartTotals(response);
                            }
                        });

                        self.showMessage('Item removed from cart', 'success');
                    } else {
                        self.showMessage(response.message || 'Failed to remove item', 'error');
                    }
                },
                error: function (xhr) {
                    let errorMessage = 'Failed to remove item from cart';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    }
                    self.showMessage(errorMessage, 'error');
                }
            });
        },

        /**
         * Update cart count in header
         */
        updateCartCount: function (count) {
            if (count === undefined) {
                // Fetch cart count from server
                $.ajax({
                    url: '/cart/count',
                    type: 'GET',
                    success: function (response) {
                        if (response && response.count !== undefined) {
                            $('.cart-count, .count-box').text(response.count);
                        }
                    }
                });
            } else {
                // Use provided count
                $('.cart-count, .count-box').text(count);
            }
        },

        /**
         * Update cart totals on page
         */
        updateCartTotals: function (data) {
            if (data.subTotal !== undefined) {
                $('.tf-totals-total-value.subtotal').text('$' + data.subTotal.toFixed(2));
            }
            if (data.taxAmount !== undefined) {
                $('.tf-totals-total-value.tax').text('$' + data.taxAmount.toFixed(2));
            }
            if (data.shippingAmount !== undefined) {
                $('.tf-totals-total-value.shipping').text('$' + data.shippingAmount.toFixed(2));
            }
            if (data.totalAmount !== undefined) {
                $('.tf-totals-total-value.total').text('$' + data.totalAmount.toFixed(2));
            }
        },

        /**
         * Show message to user
         */
        showMessage: function (message, type) {
            // Check if there's a notification system
            if (typeof toastr !== 'undefined') {
                toastr[type](message);
            } else {
                // Fallback to alert
                alert(message);
            }
        }
    };

    // Initialize cart when DOM is ready
    $(document).ready(function () {
        Cart.init();
    });

})(jQuery);
