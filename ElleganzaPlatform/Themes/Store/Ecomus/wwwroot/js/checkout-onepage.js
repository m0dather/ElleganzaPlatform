/**
 * One-Page Checkout JavaScript
 * Handles progressive form flow and AJAX updates to CheckoutSession
 */

(function() {
    'use strict';
    
    // State management
    let checkoutSessionId = null;
    let selectedShippingMethod = null;
    let selectedPaymentMethod = 1; // Default to Online Payment
    let orderTotals = {
        subtotal: 0,
        shipping: 0,
        tax: 0,
        total: 0
    };
    
    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function() {
        initializeCheckout();
    });
    
    function initializeCheckout() {
        // Parse initial totals from page
        parseInitialTotals();
        
        // Setup event handlers
        setupAddressForm();
        setupShippingSelection();
        setupPaymentSelection();
        setupConfirmForm();
    }
    
    function parseInitialTotals() {
        const subtotalEl = document.getElementById('summary-subtotal');
        const taxEl = document.getElementById('summary-tax');
        const totalEl = document.getElementById('summary-total');
        
        if (subtotalEl) {
            orderTotals.subtotal = parseFloat(subtotalEl.textContent.replace('$', '')) || 0;
        }
        if (taxEl) {
            orderTotals.tax = parseFloat(taxEl.textContent.replace('$', '')) || 0;
        }
        if (totalEl) {
            orderTotals.total = parseFloat(totalEl.textContent.replace('$', '')) || 0;
        }
    }
    
    // ========================================
    // SECTION 1: ADDRESS FORM
    // ========================================
    function setupAddressForm() {
        const form = document.getElementById('addressForm');
        const saveBtn = document.getElementById('saveAddressBtn');
        
        if (!form || !saveBtn) return;
        
        saveBtn.addEventListener('click', function(e) {
            e.preventDefault();
            saveAddress();
        });
    }
    
    function saveAddress() {
        const form = document.getElementById('addressForm');
        const saveBtn = document.getElementById('saveAddressBtn');
        const btnText = document.getElementById('addressBtnText');
        const btnSpinner = document.getElementById('addressBtnSpinner');
        
        // Validate form using HTML5 validation
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        // Show loading state
        saveBtn.disabled = true;
        btnText.style.display = 'none';
        btnSpinner.style.display = 'inline';
        
        // Prepare form data
        const formData = new FormData(form);
        
        // Make AJAX request
        fetch('/checkout/save-address', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Save session ID
                checkoutSessionId = data.sessionId;
                
                // Mark section as completed
                markSectionCompleted('section-address', 'address-status');
                
                // Enable and populate shipping section
                enableShippingSection(data.shippingMethods);
                
                // Scroll to shipping section
                scrollToSection('section-shipping');
            } else {
                // Show errors
                showErrors(data.errors);
            }
        })
        .catch(error => {
            console.error('Error saving address:', error);
            alert('An error occurred while saving your address. Please try again.');
        })
        .finally(() => {
            // Reset button state
            saveBtn.disabled = false;
            btnText.style.display = 'inline';
            btnSpinner.style.display = 'none';
        });
    }
    
    // ========================================
    // SECTION 2: SHIPPING METHOD
    // ========================================
    function enableShippingSection(shippingMethods) {
        const section = document.getElementById('section-shipping');
        const content = document.getElementById('shippingContent');
        const status = document.getElementById('shipping-status');
        const methodsList = document.getElementById('shippingMethodsList');
        
        if (!section || !content || !methodsList) return;
        
        // Populate shipping methods
        methodsList.innerHTML = '';
        shippingMethods.forEach((method, index) => {
            const methodId = method.name.replace(/\s+/g, '_').replace(/-/g, '_');
            const methodHtml = `
                <div class="fieldset-radio mb_20">
                    <input type="radio" name="ShippingMethod" id="shipping_${methodId}" 
                           value="${method.name}" data-cost="${method.cost}" 
                           class="tf-check shipping-radio" required ${index === 0 ? 'checked' : ''}>
                    <label for="shipping_${methodId}" class="text_black-2">
                        <div class="d-flex justify-content-between align-items-start w-100">
                            <div>
                                <span class="fw-6 d-block mb_5">${method.name}</span>
                                <span class="text-muted">${method.description}</span>
                                ${method.estimatedDelivery ? '<br /><small class="text-muted">Estimated delivery: ' + method.estimatedDelivery + '</small>' : ''}
                            </div>
                            <span class="fw-6 ms-3">$${method.cost.toFixed(2)}</span>
                        </div>
                    </label>
                </div>
            `;
            methodsList.innerHTML += methodHtml;
        });
        
        // Enable section
        section.classList.remove('disabled');
        section.classList.add('active');
        content.style.display = 'block';
        status.textContent = 'Active';
        status.classList.remove('bg-secondary');
        status.classList.add('bg-primary');
        
        // Auto-select first method if available
        if (shippingMethods.length > 0) {
            selectedShippingMethod = shippingMethods[0].name;
            updateShippingInSession(shippingMethods[0].name, shippingMethods[0].cost);
        }
    }
    
    function setupShippingSelection() {
        // Use event delegation since shipping methods are added dynamically
        document.addEventListener('change', function(e) {
            if (e.target.classList.contains('shipping-radio')) {
                const methodName = e.target.value;
                const methodCost = parseFloat(e.target.getAttribute('data-cost'));
                
                selectedShippingMethod = methodName;
                updateShippingInSession(methodName, methodCost);
            }
        });
    }
    
    function updateShippingInSession(methodName, methodCost) {
        const sessionIdInput = document.getElementById('shippingSessionId');
        const costInput = document.getElementById('shippingCost');
        
        if (sessionIdInput) {
            sessionIdInput.value = checkoutSessionId;
        }
        if (costInput) {
            costInput.value = methodCost;
        }
        
        // Prepare form data
        const formData = new FormData();
        formData.append('CheckoutSessionId', checkoutSessionId);
        formData.append('ShippingMethod', methodName);
        formData.append('ShippingCost', methodCost);
        
        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        formData.append('__RequestVerificationToken', token);
        
        // Make AJAX request
        fetch('/checkout/select-shipping-ajax', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Update order summary
                updateOrderSummary(data.shippingCost, data.taxAmount, data.totalAmount);
                
                // Mark section as completed
                markSectionCompleted('section-shipping', 'shipping-status');
                
                // Enable payment section
                enablePaymentSection();
                
                // Scroll to payment section
                scrollToSection('section-payment');
            } else {
                // Show errors
                const errorDiv = document.getElementById('shippingError');
                if (errorDiv) {
                    errorDiv.textContent = data.errors.join(', ');
                    errorDiv.style.display = 'block';
                }
            }
        })
        .catch(error => {
            console.error('Error updating shipping:', error);
            alert('An error occurred while updating shipping. Please try again.');
        });
    }
    
    // ========================================
    // SECTION 3: PAYMENT METHOD
    // ========================================
    function enablePaymentSection() {
        const section = document.getElementById('section-payment');
        const content = document.getElementById('paymentContent');
        const status = document.getElementById('payment-status');
        const sessionIdInput = document.getElementById('paymentSessionId');
        
        if (!section || !content) return;
        
        // Set session ID
        if (sessionIdInput) {
            sessionIdInput.value = checkoutSessionId;
        }
        
        // Enable section
        section.classList.remove('disabled');
        section.classList.add('active');
        content.style.display = 'block';
        status.textContent = 'Active';
        status.classList.remove('bg-secondary');
        status.classList.add('bg-primary');
    }
    
    function setupPaymentSelection() {
        // Use event delegation for payment method selection
        document.addEventListener('change', function(e) {
            if (e.target.classList.contains('payment-radio')) {
                const paymentMethod = parseInt(e.target.value);
                selectedPaymentMethod = paymentMethod;
                updatePaymentInSession(paymentMethod);
            }
        });
    }
    
    function updatePaymentInSession(paymentMethod) {
        // Prepare form data
        const formData = new FormData();
        formData.append('CheckoutSessionId', checkoutSessionId);
        formData.append('PaymentMethod', paymentMethod);
        
        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        formData.append('__RequestVerificationToken', token);
        
        // Make AJAX request
        fetch('/checkout/select-payment-ajax', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Mark section as completed
                markSectionCompleted('section-payment', 'payment-status');
                
                // Enable confirm section
                enableConfirmSection(paymentMethod);
                
                // Scroll to confirm section
                scrollToSection('section-confirm');
            } else {
                // Show errors
                const errorDiv = document.getElementById('paymentError');
                if (errorDiv) {
                    errorDiv.textContent = data.errors.join(', ');
                    errorDiv.style.display = 'block';
                }
            }
        })
        .catch(error => {
            console.error('Error updating payment:', error);
            alert('An error occurred while updating payment method. Please try again.');
        });
    }
    
    // ========================================
    // SECTION 4: CONFIRM ORDER
    // ========================================
    function enableConfirmSection(paymentMethod) {
        const section = document.getElementById('section-confirm');
        const content = document.getElementById('confirmContent');
        const status = document.getElementById('confirm-status');
        const sessionIdInput = document.getElementById('confirmSessionId');
        const btnText = document.getElementById('confirmBtnText');
        
        if (!section || !content) return;
        
        // Set session ID
        if (sessionIdInput) {
            sessionIdInput.value = checkoutSessionId;
        }
        
        // Update button text based on payment method
        if (btnText) {
            btnText.textContent = paymentMethod === 1 ? 'Proceed to Payment' : 'Confirm Order';
        }
        
        // Enable section
        section.classList.remove('disabled');
        section.classList.add('active');
        content.style.display = 'block';
        status.textContent = 'Active';
        status.classList.remove('bg-secondary');
        status.classList.add('bg-primary');
    }
    
    function setupConfirmForm() {
        const form = document.getElementById('confirmForm');
        const confirmBtn = document.getElementById('confirmOrderBtn');
        const btnText = document.getElementById('confirmBtnText');
        const btnSpinner = document.getElementById('confirmBtnSpinner');
        
        if (!form) return;
        
        form.addEventListener('submit', function(e) {
            // Prevent double submit
            if (confirmBtn.disabled) {
                e.preventDefault();
                return false;
            }
            
            // Show loading state
            confirmBtn.disabled = true;
            btnText.style.display = 'none';
            btnSpinner.style.display = 'inline';
            
            // Form will submit normally to backend
            return true;
        });
    }
    
    // ========================================
    // HELPER FUNCTIONS
    // ========================================
    function markSectionCompleted(sectionId, statusId) {
        const section = document.getElementById(sectionId);
        const status = document.getElementById(statusId);
        
        if (section) {
            section.classList.remove('active');
            section.classList.add('completed');
        }
        
        if (status) {
            status.textContent = 'Completed';
            status.classList.remove('bg-primary');
            status.classList.add('bg-success');
        }
    }
    
    function updateOrderSummary(shippingCost, taxAmount, totalAmount) {
        const shippingEl = document.getElementById('summary-shipping');
        const taxEl = document.getElementById('summary-tax');
        const totalEl = document.getElementById('summary-total');
        
        if (shippingEl) {
            shippingEl.textContent = '$' + shippingCost.toFixed(2);
        }
        if (taxEl) {
            taxEl.textContent = '$' + taxAmount.toFixed(2);
        }
        if (totalEl) {
            totalEl.textContent = '$' + totalAmount.toFixed(2);
        }
        
        // Update state
        orderTotals.shipping = shippingCost;
        orderTotals.tax = taxAmount;
        orderTotals.total = totalAmount;
    }
    
    function scrollToSection(sectionId) {
        const section = document.getElementById(sectionId);
        if (section) {
            setTimeout(() => {
                section.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }, 300);
        }
    }
    
    function showErrors(errors) {
        if (!errors || errors.length === 0) return;
        
        const errorMessage = errors.join('\n');
        alert('Please fix the following errors:\n\n' + errorMessage);
    }
    
})();
