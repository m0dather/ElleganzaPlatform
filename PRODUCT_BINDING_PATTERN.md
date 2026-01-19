# Product Binding Pattern for Quick Add Functionality

## Overview
To enable Quick Add functionality, all product cards must include `data-product-id` attributes.
The productId must be accessible by the JavaScript cart.js module.

## Implementation Pattern

### Option 1: Add to Parent Card Container (RECOMMENDED)
Add `data-product-id` to the main product card wrapper:

```html
<div class="card-product fl-item" data-product-id="@product.Id">
```

### Option 2: Add to Quick Add Button Directly
Add `data-product-id` directly on the quick-add button:

```html
<a href="#quick_add" data-bs-toggle="modal"
   class="box-icon bg_white quick-add tf-btn-loading"
   data-product-id="@product.Id"
   data-product-name="@product.Name"
   data-product-price="@product.Price">
    <span class="icon icon-bag"></span>
    <span class="tooltip">Quick Add</span>
</a>
```

### Option 3: Both (BEST FOR COMPATIBILITY)
Add to both the card and the button for maximum compatibility:

```html
<!-- Parent card with product ID -->
<div class="card-product fl-item" data-product-id="@product.Id">
    <div class="card-product-wrapper">
        <!-- Product image, etc. -->
        <div class="list-product-btn">
            <!-- Quick add button with product ID -->
            <a href="#quick_add" data-bs-toggle="modal"
               class="box-icon bg_white quick-add tf-btn-loading"
               data-product-id="@product.Id">
                <span class="icon icon-bag"></span>
                <span class="tooltip">Quick Add</span>
            </a>
        </div>
    </div>
</div>
```

## Files Requiring Updates

### Static Demo Views (Using Placeholder IDs)
These views currently use static HTML and should be updated with placeholder IDs (1, 2, 3, etc.):

1. `/Themes/Store/Ecomus/Views/Home/Index.cshtml` - 12 products
2. `/Themes/Store/Ecomus/Views/Shop/Product.cshtml` - Related products section
3. `/Themes/Store/Ecomus/Views/Shared/_Header.cshtml` - Header product carousel
4. `/Themes/Store/Ecomus/Views/Account/Wishlist.cshtml` - Wishlist products

### Dynamic Views (Using Real Data from Backend)
When the backend provides product data through Models, use:

```cshtml
@foreach (var product in Model.Products)
{
    <div class="card-product fl-item" data-product-id="@product.Id">
        <!-- Product card content -->
        <div class="list-product-btn">
            <a href="#quick_add" data-bs-toggle="modal"
               class="box-icon bg_white quick-add tf-btn-loading"
               data-product-id="@product.Id"
               data-product-name="@product.Name"
               data-product-price="@product.Price">
                <span class="icon icon-bag"></span>
                <span class="tooltip">Quick Add</span>
            </a>
        </div>
    </div>
}
```

## JavaScript Cart Handler

The cart.js module already handles product ID extraction:

```javascript
// Quick Add to cart button (on product cards)
$(document).on('click', '#quick_add, .quick-add, a[href="#quick_add"]', function (e) {
    e.preventDefault();
    e.stopPropagation();
    
    const $btn = $(this);
    
    // Try to get product ID from multiple sources:
    // 1. Direct data attribute on button
    // 2. Data attribute on parent card
    // 3. Data attribute on closest product container
    let productId = $btn.data('product-id') || 
                   $btn.data('productId') ||
                   $btn.closest('.card-product, .product-item, [data-product-id]').data('product-id') ||
                   $btn.closest('.card-product, .product-item, [data-product-id]').data('productId');
    
    // Validate product ID exists
    if (!productId) {
        window.UI.notify.error('Unable to add product. Product information is missing.');
        return;
    }
    
    self.addToCart(productId, quantity, $btn);
});
```

## Testing

After adding product IDs, test:

1. **Quick Add works**: Click Quick Add button on any product card
2. **Product is added**: Check cart count increases
3. **Correct product**: Verify the right product appears in mini-cart
4. **Error handling**: Remove data-product-id temporarily and verify themed error appears

## Notes

- **DO NOT** rely on DOM traversal to guess productId
- **ALWAYS** provide explicit data-product-id attributes
- **Static demo pages**: Use sequential IDs (1, 2, 3, ...) for demonstration
- **Production pages**: Use actual @product.Id from backend Model
