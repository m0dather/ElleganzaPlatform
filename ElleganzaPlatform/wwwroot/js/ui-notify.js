/**
 * UI Notification System
 * Ecomus-themed notification helper for displaying success, error, and warning messages
 * Uses Bootstrap 5 toast component styled to match Ecomus theme
 */

(function ($) {
    'use strict';

    // Create notification container on page load
    $(document).ready(function () {
        if ($('#notification-container').length === 0) {
            $('body').append(`
                <div id="notification-container" 
                     class="toast-container position-fixed top-0 end-0 p-3" 
                     style="z-index: 9999;">
                </div>
            `);
        }
    });

    // UI Notification Helper
    window.UI = window.UI || {};
    window.UI.notify = {
        /**
         * Show success notification
         * @param {string} message - The message to display
         * @param {number} duration - Duration in milliseconds (default: 4000)
         */
        success: function (message, duration) {
            this._show(message, 'success', duration);
        },

        /**
         * Show error notification
         * @param {string} message - The message to display
         * @param {number} duration - Duration in milliseconds (default: 5000)
         */
        error: function (message, duration) {
            this._show(message, 'error', duration || 5000);
        },

        /**
         * Show warning notification
         * @param {string} message - The message to display
         * @param {number} duration - Duration in milliseconds (default: 4000)
         */
        warning: function (message, duration) {
            this._show(message, 'warning', duration);
        },

        /**
         * Show info notification
         * @param {string} message - The message to display
         * @param {number} duration - Duration in milliseconds (default: 4000)
         */
        info: function (message, duration) {
            this._show(message, 'info', duration);
        },

        /**
         * Show confirmation dialog (non-blocking, uses Bootstrap modal)
         * @param {string} message - The confirmation message
         * @param {Function} onConfirm - Callback when user confirms
         * @param {Function} onCancel - Callback when user cancels (optional)
         */
        confirm: function (message, onConfirm, onCancel) {
            // Remove any existing confirmation modal
            $('#confirmationModal').remove();

            // Create confirmation modal
            const modalHtml = `
                <div class="modal fade" id="confirmationModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header border-0 pb-0">
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body text-center pt-0">
                                <div class="mb-3">
                                    <i class="icon icon-question-circle" style="font-size: 48px; color: #ffb321;"></i>
                                </div>
                                <p class="fs-16 mb-4">${message}</p>
                            </div>
                            <div class="modal-footer border-0 justify-content-center gap-2">
                                <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancel</button>
                                <button type="button" class="btn btn-primary" id="confirmButton">Confirm</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            $('body').append(modalHtml);
            const modal = new bootstrap.Modal(document.getElementById('confirmationModal'));

            // Handle confirm button
            $('#confirmButton').on('click', function () {
                modal.hide();
                if (typeof onConfirm === 'function') {
                    onConfirm();
                }
            });

            // Handle cancel (modal close)
            $('#confirmationModal').on('hidden.bs.modal', function () {
                const wasConfirmed = $(this).data('confirmed');
                if (!wasConfirmed && typeof onCancel === 'function') {
                    onCancel();
                }
                $(this).remove();
            });

            // Mark as confirmed when button is clicked
            $('#confirmButton').on('click', function () {
                $('#confirmationModal').data('confirmed', true);
            });

            modal.show();
        },

        /**
         * Internal method to show notification
         * @private
         */
        _show: function (message, type, duration) {
            duration = duration || 4000;

            // Icon and color mapping
            const config = {
                success: { icon: 'icon-check-circle', bgClass: 'bg-success' },
                error: { icon: 'icon-close-circle', bgClass: 'bg-danger' },
                warning: { icon: 'icon-warning', bgClass: 'bg-warning' },
                info: { icon: 'icon-info-circle', bgClass: 'bg-info' }
            };

            const typeConfig = config[type] || config.info;
            const toastId = 'toast-' + Date.now();

            // Create toast HTML
            const toastHtml = `
                <div id="${toastId}" class="toast align-items-center text-white border-0 ${typeConfig.bgClass}" 
                     role="alert" aria-live="assertive" aria-atomic="true" 
                     data-bs-autohide="true" data-bs-delay="${duration}">
                    <div class="d-flex">
                        <div class="toast-body d-flex align-items-center gap-2">
                            <i class="${typeConfig.icon}" style="font-size: 20px;"></i>
                            <span>${message}</span>
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                                data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                </div>
            `;

            // Append to container
            $('#notification-container').append(toastHtml);

            // Initialize and show toast
            const toastElement = document.getElementById(toastId);
            const toast = new bootstrap.Toast(toastElement);
            toast.show();

            // Remove from DOM after hidden
            toastElement.addEventListener('hidden.bs.toast', function () {
                $(this).remove();
            });
        }
    };

})(jQuery);
