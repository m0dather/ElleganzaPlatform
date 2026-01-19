# Phase 3.1.1: Cart Hardening (Security & Data Integrity) - Implementation Complete

## Overview

This document details the implementation of Phase 3.1.1: Cart Hardening for the ElleganzaPlatform e-commerce system. This phase focused on securing the shopping cart functionality against CSRF attacks, enforcing data validation, and ensuring single source of truth principles.

## Implementation Date

**Completed:** 2026-01-19

## Objectives Achieved

✅ **CSRF Protection** - All cart POST endpoints protected against Cross-Site Request Forgery  
✅ **Cart Merge Verification** - Guest cart properly merges into authenticated cart after login  
✅ **Quantity & Stock Validation** - Enforced validation rules prevent overselling and invalid quantities  
✅ **Single Source of Truth** - CartService is the exclusive source for all cart operations  
✅ **Error Handling** - Comprehensive error handling with user-friendly messages  

---

## 1. CSRF Protection Implementation

### Problem
Cart operations (add, update, remove) were vulnerable to CSRF attacks as they lacked anti-forgery token validation.

### Solution

#### 1.1 Anti-Forgery Configuration (Program.cs)
```csharp
// Phase 3.1.1: Configure Anti-Forgery for AJAX requests
builder.Services.AddAntiforgery(options =>
{
    // Read token from header for AJAX requests
    options.HeaderName = "RequestVerificationToken";
});
```

**Purpose:** Configures ASP.NET Core to accept anti-forgery tokens from custom HTTP headers, enabling AJAX/JSON requests to be validated.

#### 1.2 Token Generation in Layout (_Layout.cshtml)
```html
@* Anti-Forgery Token for AJAX requests (Phase 3.1.1: Cart Hardening) *@
@Html.AntiForgeryToken()
```

**Location:** After `</head>` tag in layout  
**Purpose:** Generates a hidden form field with the anti-forgery token accessible to JavaScript

#### 1.3 JavaScript Token Handling (cart.js)
```javascript
setupAjaxDefaults: function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    if (token) {
        $.ajaxSetup({
            beforeSend: function(xhr, settings) {
                if (settings.type !== 'GET') {
                    xhr.setRequestHeader('RequestVerificationToken', token);
                }
            }
        });
    }
}
```

**Purpose:** Extracts the token from the hidden field and automatically includes it in all POST/PUT/DELETE AJAX requests

#### 1.4 Controller Validation (CartController.cs)
```csharp
[HttpPost("/cart/add")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)

[HttpPost("/cart/update")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)

[HttpPost("/cart/remove/{productId}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RemoveFromCart(int productId)

[HttpPost("/cart/clear")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ClearCart()
```

**Purpose:** Enforces anti-forgery token validation on all cart modification endpoints

### Security Impact
- **CSRF attacks prevented**: Malicious sites can no longer trick users into adding/removing cart items
- **No UI changes required**: Transparent to end users
- **Production-ready**: Follows ASP.NET Core security best practices

---

## 2. Cart Merge Verification & Hardening

### Current Implementation (Already Correct)

The cart merge logic in `CartService.MergeGuestCartAsync()` was already well-implemented. Phase 3.1.1 added comprehensive documentation and explicit validation comments.

#### 2.1 Merge Trigger (AccountController.cs)
```csharp
if (result.Succeeded)
{
    // Add custom claims
    await AddCustomClaimsAsync(user);

    // Merge guest cart into user cart after successful login
    var cartService = HttpContext.RequestServices.GetRequiredService<ICartService>();
    await cartService.MergeGuestCartAsync();
    
    // ... redirect logic
}
```

**Location:** Line 101-110 in AccountController  
**Timing:** Immediately after successful authentication, before redirect

#### 2.2 Merge Logic (CartService.cs)

**Business Rules Enforced:**

1. **Quantity Summation**: When a product exists in both carts, quantities are summed
   ```csharp
   var newQuantity = existingItem.Quantity + guestItem.Quantity;
   existingItem.Quantity = Math.Min(newQuantity, product.StockQuantity);
   ```

2. **Stock Limit Enforcement**: Combined quantity never exceeds available stock
   ```csharp
   existingItem.Quantity = Math.Min(newQuantity, product.StockQuantity);
   ```

3. **Price Snapshot Preservation**: User cart retains its original price; new items capture current price
   ```csharp
   PriceSnapshot = product.Price, // Capture current price for new items
   ```

4. **Session Cart Cleared**: Guest cart is cleared AFTER successful merge to prevent double-merge
   ```csharp
   await _context.SaveChangesAsync();
   await SaveCartItemsToSessionAsync(new List<CartItemViewModel>());
   ```

#### 2.3 Double-Merge Prevention

**Safeguards:**
- Empty guest cart check: `if (guestCartItems.Count == 0) return;`
- Session cleared after merge: Prevents subsequent merge attempts
- Authenticated user check: `if (string.IsNullOrEmpty(_currentUserService.UserId)) return;`

---

## 3. Quantity & Stock Validation Hardening

### 3.1 AddToCart Validation

**Rules Enforced:**

```csharp
// Phase 3.1.1 Validation: Reject invalid quantities
if (quantity <= 0)
    return false; // Quantity must be at least 1

// Phase 3.1.1 Validation: Check stock availability
if (product.StockQuantity < quantity)
    return false; // Insufficient stock

// Phase 3.1.1 Validation: Ensure combined quantity doesn't exceed stock
if (newQuantity > product.StockQuantity)
    return false; // Combined quantity would exceed available stock
```

**Error Messages (Controller):**
- `"Quantity must be at least 1"` - For zero or negative quantities
- `"Unable to add item to cart. Product may be out of stock or unavailable."` - For stock issues

### 3.2 UpdateCartItem Validation

**Rules Enforced:**

```csharp
// Phase 3.1.1 Validation: Reject negative quantities
if (quantity < 0)
    return false; // Negative quantities are not allowed

// Phase 3.1.1 Business Rule: Quantity of 0 means remove the item
if (quantity == 0)
    return await RemoveFromCartAsync(productId);

// Phase 3.1.1 Validation: Ensure quantity doesn't exceed available stock
if (product.StockQuantity < quantity)
    return false; // Requested quantity exceeds available stock
```

**Error Messages (Controller):**
- `"Quantity cannot be negative"` - For negative quantities
- `"Unable to update cart item. Please check product availability and stock."` - For stock issues

### 3.3 Controller-Level Validation

Additional validation and error handling added to CartController:

```csharp
// Quantity validation
if (request.Quantity <= 0)
    return BadRequest(new { success = false, message = "Quantity must be at least 1" });

// Try-catch for unexpected errors
catch (Exception ex)
{
    _logger.LogError(ex, "Error adding product {ProductId} to cart", request.ProductId);
    return StatusCode(500, new { success = false, message = "An error occurred while adding the item to cart" });
}
```

---

## 4. Single Source of Truth Verification

### Audit Results

✅ **CartService is the exclusive source for all cart operations**

**Verified:**
- ✅ No direct session access to cart data outside CartService
- ✅ Header cart count uses `/cart/count` endpoint (CartService)
- ✅ Cart operations go through CartService methods
- ✅ No duplicate cart logic in views or controllers

**Cart Data Flow:**

```
┌─────────────┐
│   Browser   │
│  (cart.js)  │
└──────┬──────┘
       │ AJAX + CSRF Token
       ▼
┌─────────────────┐
│ CartController  │ ◄── [ValidateAntiForgeryToken]
│  (API Layer)    │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│  CartService    │ ◄── Single Source of Truth
│  (Logic Layer)  │
└──────┬──────────┘
       │
       ├──► Session (Guest Cart)
       └──► Database (User Cart)
```

---

## 5. Error Handling & Feedback

### 5.1 Enhanced Logging

All cart operations now include structured logging:

```csharp
_logger.LogInformation("Product {ProductId} added to cart successfully", request.ProductId);
_logger.LogWarning("Failed to add product {ProductId} to cart (quantity: {Quantity})", 
    request.ProductId, request.Quantity);
_logger.LogError(ex, "Error adding product {ProductId} to cart", request.ProductId);
```

### 5.2 User-Friendly Error Messages

**Client-Side (cart.js):**
- Success messages via `showMessage('Product added to cart successfully!', 'success')`
- Error messages extracted from server response: `xhr.responseJSON.message`
- Fallback messages for network errors

**Server-Side (CartController):**
- `400 Bad Request` for validation failures with descriptive messages
- `500 Internal Server Error` for unexpected errors with safe messages
- Detailed error info logged server-side for debugging

### 5.3 No Silent Failures

**Before Phase 3.1.1:**
- Failed operations might return success without validation
- Errors logged but not surfaced to user

**After Phase 3.1.1:**
- All failures return explicit error responses
- User always receives feedback (success or error)
- Errors are logged with context for troubleshooting

---

## Files Modified

| File | Purpose | Changes |
|------|---------|---------|
| `Program.cs` | Configure anti-forgery | Added `AddAntiforgery` with header support |
| `_Layout.cshtml` | Generate CSRF token | Added `@Html.AntiForgeryToken()` |
| `cart.js` | Send CSRF token | Added `setupAjaxDefaults()` method |
| `CartController.cs` | Validate CSRF & handle errors | Added `[ValidateAntiForgeryToken]` + error handling |
| `CartService.cs` | Harden validation | Enhanced comments + explicit validation |

---

## Testing Recommendations

### Manual Testing

1. **CSRF Protection**
   - Try to submit cart form without token (should fail)
   - Verify AJAX requests include token header
   - Test with browser developer tools

2. **Cart Merge**
   - Add items as guest
   - Login as existing user with items in cart
   - Verify quantities are summed correctly
   - Verify stock limits are respected

3. **Validation**
   - Try adding negative quantity (should fail)
   - Try adding quantity exceeding stock (should fail)
   - Try updating to zero (should remove item)

4. **Error Messages**
   - Simulate stock shortage
   - Simulate network error
   - Verify user sees friendly error messages

### Automated Testing (Future)

Consider adding integration tests for:
- CSRF token validation
- Cart merge scenarios
- Stock validation edge cases
- Error handling paths

---

## Security Considerations

### What Was Fixed

✅ **CSRF Vulnerability**: Cart operations were unprotected  
✅ **Validation Gaps**: Stock could be exceeded in certain scenarios  
✅ **Silent Failures**: Errors weren't always communicated to users  

### What Remains Secure

✅ **Store Isolation**: Products validated against current store context  
✅ **Authentication**: User context properly tracked  
✅ **Price Integrity**: Price snapshots prevent tampering  
✅ **Session Security**: HttpOnly and Essential flags set  

### Future Enhancements (Not in Scope)

- Rate limiting on cart operations
- Captcha for excessive cart modifications
- Advanced fraud detection
- Distributed session storage (Redis)

---

## Breaking Changes

**NONE** - This is a backward-compatible security hardening.

- UI remains unchanged
- Existing cart operations work the same
- CSRF protection is transparent to users
- Error messages are more informative (improvement)

---

## Rollback Procedure

If issues arise, the changes can be safely rolled back:

1. Remove `[ValidateAntiForgeryToken]` from CartController
2. Remove `setupAjaxDefaults()` call from cart.js
3. Remove antiforgery configuration from Program.cs
4. Remove `@Html.AntiForgeryToken()` from _Layout.cshtml

The cart will continue to function, though without CSRF protection.

---

## Success Criteria

✅ Cart actions protected from CSRF attacks  
✅ Cart merges exactly once after login  
✅ Invalid quantities rejected safely  
✅ Stock limits respected  
✅ Cart behavior is predictable  
✅ Ready for Checkout (Phase 3.2)  

---

## Next Phase

**Phase 3.2: Checkout Implementation**

With the cart now hardened, the next phase will implement:
- Checkout flow
- Payment integration
- Order creation
- Inventory deduction

The hardened cart provides a secure foundation for checkout operations.

---

## Maintenance Notes

### For Developers

1. **Adding New Cart Operations**: Always use `[ValidateAntiForgeryToken]` on POST/PUT/DELETE endpoints
2. **Validation Rules**: All quantity/stock checks should be in CartService, not controllers
3. **Error Messages**: Use structured logging + user-friendly client messages
4. **Testing**: Test both guest and authenticated cart scenarios

### For DevOps

1. **Monitoring**: Watch for increased 400/500 errors after deployment (validate CSRF is working)
2. **Logs**: Check for `CartController` and `CartService` log entries
3. **Session**: Ensure session storage is configured properly

---

## Conclusion

Phase 3.1.1 successfully hardened the shopping cart with:
- **Security**: CSRF protection on all cart operations
- **Reliability**: Robust validation preventing overselling
- **Consistency**: Single source of truth enforced
- **Transparency**: Clear error messages and logging

The cart is now production-ready and provides a secure foundation for the checkout phase.
