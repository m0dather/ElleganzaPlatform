# Mini Cart Off-Canvas - Visual Design Specification

## Overview
This document describes the visual design and user experience of the mini cart off-canvas slider implemented for ElleganzaPlatform's Ecomus theme.

## Layout Structure

```
┌─────────────────────────────────────────────────────┐
│  Header                                         [×]  │
│  ───────────────────────────────────────────────────│
│  Shopping Cart (3)                                  │
│  ═══════════════════════════════════════════════════│
│                                                     │
│  ┌────┐                                             │
│  │img │  Product Name                          [×]  │
│  │80px│  [−] 2 [+]            $29.99               │
│  └────┘                                             │
│  ─────────────────────────────────────────────────  │
│  ┌────┐                                             │
│  │img │  Another Product                       [×]  │
│  │80px│  [−] 1 [+]            $49.99               │
│  └────┘                                             │
│  ─────────────────────────────────────────────────  │
│  ┌────┐                                             │
│  │img │  Third Product                         [×]  │
│  │80px│  [−] 3 [+]            $89.97               │
│  └────┘                                             │
│                                                     │
│  ═══════════════════════════════════════════════════│
│  Subtotal:                              $169.95     │
│  Tax:                                    $16.99     │
│  Shipping:                                 Free     │
│  ─────────────────────────────────────────────────  │
│  Total:                                 $186.94     │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │          View Cart                          │   │
│  └─────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────┐   │
│  │          Checkout                           │   │
│  └─────────────────────────────────────────────┘   │
│            Clear Cart                               │
└─────────────────────────────────────────────────────┘
```

## Empty State

```
┌─────────────────────────────────────────────────────┐
│  Header                                         [×]  │
│  ───────────────────────────────────────────────────│
│                                                     │
│                                                     │
│                     🛍️                              │
│                  (icon-bag)                         │
│                                                     │
│              Your cart is empty                     │
│                                                     │
│        ┌────────────────────────┐                  │
│        │  Continue Shopping      │                  │
│        └────────────────────────┘                  │
│                                                     │
│                                                     │
└─────────────────────────────────────────────────────┘
```

## Visual Specifications

### Container
- **Position**: Fixed right
- **Width**: Responsive (Bootstrap offcanvas default ~400px)
- **Height**: 100vh (full viewport height)
- **Background**: White (#ffffff)
- **Shadow**: Left shadow for depth
- **Animation**: Slide in from right (Bootstrap default)
- **Overlay**: Semi-transparent dark overlay on main content

### Header Section
- **Background**: White
- **Border Bottom**: 1px solid #e5e5e5
- **Padding**: 20px
- **Close Button**: Top right corner
  - Icon: × (times/close)
  - Color: Dark gray
  - Hover: Darker

### Shopping Cart Title
- **Text**: "Shopping Cart ({count})"
- **Font Size**: h5
- **Font Weight**: Semi-bold
- **Color**: Dark text
- **Margin Bottom**: 0

### Cart Items Section
- **Max Height**: 400px
- **Overflow**: Scroll (vertical only)
- **Padding**: 20px
- **Background**: White

### Individual Cart Item
- **Layout**: Flex (row)
- **Gap**: 12px (gap-3)
- **Margin Bottom**: 12px
- **Padding Bottom**: 12px
- **Border Bottom**: 1px solid #e5e5e5

#### Product Image
- **Size**: 80px × 80px
- **Object Fit**: Cover
- **Border Radius**: 4px (optional)
- **Hover**: None (image is link)

#### Product Info
- **Flex Grow**: 1 (takes remaining space)

#### Product Name
- **Font Weight**: Semi-bold (600)
- **Font Size**: 14px
- **Color**: Dark text
- **Text Decoration**: None
- **Hover**: Underline
- **Link**: Yes (to product page)

#### Remove Button
- **Position**: Top right of item
- **Style**: Link button
- **Color**: Red/danger
- **Icon**: × (close)
- **Size**: Small
- **Hover**: Darker red

#### Quantity Controls
- **Layout**: Flex (horizontal)
- **Alignment**: Center
- **Buttons**: 
  - Style: btn-sm btn-outline-secondary
  - Text: - and +
  - Width: Auto
  - Height: Auto
  - Border Radius: 4px
- **Input**:
  - Width: 50px
  - Text Align: Center
  - Background: White
  - Border: 1px solid #ddd
  - Readonly: Yes
  - Margin: 0 4px

#### Item Price
- **Font Weight**: Semi-bold
- **Font Size**: 14px
- **Color**: Dark text
- **Format**: $XX.XX

### Footer Section
- **Padding**: 20px
- **Background**: White
- **Border Top**: 1px solid #e5e5e5

### Totals Section
- **Margin Bottom**: 20px

#### Subtotal/Tax/Shipping Rows
- **Layout**: Flex (justify-content: space-between)
- **Margin Bottom**: 8px
- **Font Size**: 14px
- **Color**: Dark text

#### Total Row
- **Layout**: Flex (justify-content: space-between)
- **Margin Bottom**: 20px
- **Font Weight**: Bold
- **Font Size**: 18px
- **Color**: Dark text
- **Padding Top**: 12px
- **Border Top**: 1px solid #ddd

### Action Buttons

#### View Cart Button
- **Style**: btn-outline-dark
- **Width**: 100%
- **Margin Bottom**: 8px
- **Text**: "View Cart"
- **Border Radius**: 3px
- **Hover**: Fill with dark background

#### Checkout Button
- **Style**: btn-fill (primary/dark)
- **Width**: 100%
- **Margin Bottom**: 8px
- **Text**: "Checkout"
- **Background**: Dark
- **Color**: White
- **Border Radius**: 3px
- **Hover**: Slightly darker

#### Clear Cart Link
- **Style**: Link button
- **Width**: 100%
- **Text Align**: Center
- **Color**: Red/danger
- **Text Decoration**: None
- **Font Size**: 14px
- **Hover**: Underline

## Interactions & Animations

### Opening
1. User clicks cart icon in header
2. Dark overlay fades in (0.3s)
3. Cart slides in from right (0.3s)
4. Cart content loads (spinner while loading)

### Loading State
- Spinner in center of content area
- Light gray background
- Text: "Loading..."

### Quantity Change
1. User clicks + or - button
2. Input value updates immediately
3. AJAX call to server (in background)
4. On success: Cart reloads, header count updates
5. On error: Show error message, revert input

### Remove Item
1. User clicks × button
2. Confirmation dialog: "Remove this item from cart?"
3. On confirm: AJAX call
4. On success: Cart reloads with animation
5. Item count updates in header

### Clear Cart
1. User clicks "Clear Cart"
2. Confirmation: "Are you sure you want to clear your entire cart?"
3. On confirm: AJAX call
4. On success: Show empty state
5. Header count updates to 0

### Navigation
- "View Cart" → Navigate to /cart
- "Checkout" → Navigate to /checkout
- Product name/image → Navigate to product page
- "Continue Shopping" (empty state) → Navigate to /shop

### Closing
1. User clicks × button OR
2. User clicks outside cart (on overlay) OR
3. User presses ESC key (Bootstrap default)
4. Cart slides out to right (0.3s)
5. Overlay fades out (0.3s)

## Responsive Behavior

### Desktop (> 768px)
- Width: ~400px
- Full features as described

### Tablet (768px - 1024px)
- Width: ~350px
- Same functionality

### Mobile (< 768px)
- Width: 85% of screen width
- Product images: 60px × 60px (smaller)
- Font sizes slightly reduced
- Same functionality

## Browser Compatibility
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+
- Requires JavaScript enabled
- Requires Bootstrap 5

## Accessibility
- Keyboard navigable (TAB key)
- ESC key closes cart
- ARIA labels on buttons
- Focus management
- Screen reader compatible
- High contrast text

## Performance
- Lazy load product images
- Debounce quantity updates
- Minimal DOM manipulation
- CSS transitions (hardware accelerated)
- AJAX calls with loading states

## User Experience Notes

### Feedback
- Loading states for all operations
- Success messages for add/remove
- Error messages for failures
- Disabled states during operations

### Smooth Transitions
- All state changes animated
- No sudden content jumps
- Scroll position maintained where possible
- Loading indicators

### Error Handling
- Network errors shown to user
- Validation errors explained
- Retry mechanisms where appropriate
- Graceful degradation

## Color Scheme (Based on Ecomus Theme)
- Primary Background: #ffffff (white)
- Secondary Background: #f8f9fa (light gray)
- Border Color: #e5e5e5 (light gray)
- Text Primary: #212529 (dark)
- Text Secondary: #6c757d (gray)
- Danger/Remove: #dc3545 (red)
- Success: #28a745 (green)
- Primary Action: #212529 (dark)

## Typography
- Font Family: System default / Ecomus theme font
- Base Font Size: 14px
- Headings: Semi-bold (600)
- Body: Regular (400)
- Prices: Semi-bold (600)
- Buttons: Medium (500)

---

**Note**: This design maintains consistency with the existing Ecomus theme while providing a modern, user-friendly shopping cart experience. All interactions are smooth, responsive, and provide clear feedback to the user.
