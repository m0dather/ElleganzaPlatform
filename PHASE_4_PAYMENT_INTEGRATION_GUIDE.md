# Phase 4: Payment Integration - Implementation Guide

## Overview

This document describes the complete payment integration implementation for ElleganzaPlatform using Stripe Checkout. The system is built with security, extensibility, and provider-agnostic architecture in mind.

## Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────┐
│          Web Layer (Controllers)            │
│  - PaymentController (Payment Initiation)   │
│  - WebhookController (Webhook Processing)   │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│       Application Layer (Abstractions)      │
│  - IPaymentService Interface                │
│  - PaymentResult DTO                        │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│    Infrastructure Layer (Implementation)    │
│  - StripePaymentService                     │
│  - Webhook Signature Verification           │
│  - Idempotent Processing                    │
└─────────────────────────────────────────────┘
```

## Features Implemented

### ✅ Core Payment Functionality

1. **Payment Abstractions** (`IPaymentService`)
   - `CreatePaymentAsync(orderId)` - Creates Stripe Checkout session
   - `VerifyPaymentAsync(payload, signature)` - Processes webhooks

2. **Payment Result Model**
   - IsSuccess: Boolean flag for operation outcome
   - Provider: Payment provider name (e.g., "Stripe")
   - TransactionId: Unique transaction identifier
   - ErrorMessage: Error details if failed
   - CheckoutUrl: Stripe Checkout URL for payment

3. **Order Status Updates**
   - `Pending` → Payment awaiting (initial state)
   - `Paid` → Payment successful
   - `Failed` → Payment failed
   - Existing statuses: Processing, Shipped, Delivered, Cancelled, Refunded

### ✅ Security Features

1. **Webhook Signature Verification**
   - Validates all incoming webhooks using Stripe signature
   - Rejects unsigned or tampered requests
   - Located in: `StripePaymentService.VerifyPaymentAsync()`

2. **Idempotent Webhook Processing**
   - Safely handles duplicate webhook deliveries
   - Checks existing order status and transaction ID
   - Prevents double-charging or status corruption

3. **Order Access Control**
   - Customers can only pay for their own orders
   - Verified at controller level before payment creation
   - Uses `IOrderService.GetOrderDetailsAsync()` for validation

4. **Payment Safety Rules**
   - Orders can only be paid once
   - Only orders with `Status = Pending` can be paid
   - Orders with existing transaction IDs are rejected

### ✅ UI Integration

**Customer-Facing**
- "Pay Now" button on Order Success page
- "Pay Now" button on Orders list page
- "Pay Now" button on Order Details page
- Buttons only visible when `Order.Status == Pending`

**Admin/Vendor Views**
- Payment transaction ID displayed when available
- Updated status badges for Paid/Failed statuses
- Read-only payment information (no payment actions)

## Configuration

### 1. Stripe API Keys

Update `appsettings.json` with your Stripe credentials:

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_your_publishable_key_here",
    "SecretKey": "sk_test_your_secret_key_here",
    "WebhookSecret": "whsec_your_webhook_secret_here"
  },
  "App": {
    "BaseUrl": "https://localhost:7000"
  }
}
```

**IMPORTANT:** 
- Never commit real API keys to source control
- Use environment variables or Azure Key Vault in production
- Keep webhook secrets secure

### 2. Database Migration

Run the migration to add payment fields:

```bash
cd ElleganzaPlatform
dotnet ef database update --project ../ElleganzaPlatform.Infrastructure
```

This adds:
- `PaymentTransactionId` column to Orders table
- `Paid` and `Failed` enum values to OrderStatus

### 3. Webhook Configuration

#### Local Testing with Stripe CLI

1. Install Stripe CLI: https://stripe.com/docs/stripe-cli
2. Login: `stripe login`
3. Forward webhooks:
   ```bash
   stripe listen --forward-to https://localhost:7000/payments/webhook/stripe
   ```
4. Copy the webhook signing secret to `appsettings.json`

#### Production Webhook Setup

1. Go to Stripe Dashboard → Developers → Webhooks
2. Add endpoint: `https://yourdomain.com/payments/webhook/stripe`
3. Select event: `checkout.session.completed`
4. Copy webhook signing secret to configuration

## Payment Flow

### Customer Journey

```
1. Customer places order
   └─> Order created with Status = Pending

2. Customer clicks "Pay Now"
   └─> POST /payment/create?orderId=X
       ├─> Validates order belongs to user
       ├─> Validates order Status = Pending
       └─> Creates Stripe Checkout session

3. Customer redirected to Stripe
   └─> Enters payment information
       ├─> Success → redirected to Order Success page
       └─> Cancel → redirected to Order Success page

4. Stripe sends webhook
   └─> POST /payments/webhook/stripe
       ├─> Verifies webhook signature
       ├─> Checks payment status
       ├─> Updates Order.Status = Paid
       ├─> Stores Order.PaymentTransactionId
       └─> Returns 200 OK
```

### Critical Security Points

**1. Webhook is Source of Truth**
- Never trust client redirects for payment confirmation
- Always wait for webhook to update order status
- Client success/cancel URLs are for UX only

**2. Signature Verification**
```csharp
Event stripeEvent = EventUtility.ConstructEvent(
    payload,           // Raw webhook body
    signature,         // Stripe-Signature header
    webhookSecret      // From configuration
);
```

**3. Idempotency Check**
```csharp
if (order.Status == OrderStatus.Paid && 
    order.PaymentTransactionId == session.PaymentIntentId)
{
    // Duplicate webhook - safely ignore
    return success;
}
```

## Code Structure

### Files Added/Modified

**Domain Layer:**
- `ElleganzaPlatform.Domain/Entities/Order.cs` - Added PaymentTransactionId
- `ElleganzaPlatform.Domain/Enums/CommonEnums.cs` - Added Paid/Failed statuses

**Application Layer:**
- `ElleganzaPlatform.Application/Services/IPaymentService.cs` - Payment interface
- `ElleganzaPlatform.Application/ViewModels/*` - Added CanBePaid properties

**Infrastructure Layer:**
- `ElleganzaPlatform.Infrastructure/Services/Payment/StripePaymentService.cs` - Stripe implementation
- `ElleganzaPlatform.Infrastructure/DependencyInjection.cs` - Service registration
- `ElleganzaPlatform.Infrastructure/Services/Application/*Service.cs` - Updated to include payment fields

**Web Layer:**
- `ElleganzaPlatform/Controllers/PaymentController.cs` - Payment initiation
- `ElleganzaPlatform/Controllers/WebhookController.cs` - Webhook handling
- `ElleganzaPlatform/Themes/Store/Ecomus/Views/**/*.cshtml` - UI updates
- `ElleganzaPlatform/Areas/Admin/Store/Views/Orders/*.cshtml` - Admin UI updates

## Testing Checklist

### Manual Testing

- [ ] **Order Creation**: Place an order → verify Status = Pending
- [ ] **Pay Now Visibility**: Verify button shows only for Pending orders
- [ ] **Payment Creation**: Click "Pay Now" → redirected to Stripe
- [ ] **Payment Success**: Complete payment → webhook updates Status = Paid
- [ ] **Payment Failure**: Cancel payment → verify order remains Pending
- [ ] **Transaction ID**: Verify transaction ID stored and displayed
- [ ] **Duplicate Webhook**: Send webhook twice → verify idempotent handling
- [ ] **Invalid Signature**: Send webhook with bad signature → rejected
- [ ] **Access Control**: Customer A cannot pay Customer B's orders
- [ ] **Double Payment**: Try paying Paid order → rejected
- [ ] **Admin View**: Verify admin sees payment status (read-only)
- [ ] **Vendor View**: Verify vendor sees payment status (read-only)

### Test Webhook Locally

Using Stripe CLI:
```bash
# Trigger successful payment
stripe trigger checkout.session.completed

# Or send custom webhook
stripe events resend evt_xxxx
```

## Provider-Agnostic Design

### Adding New Payment Providers

To add PayPal, Square, or other providers:

1. **Create Implementation**
   ```csharp
   public class PayPalPaymentService : IPaymentService
   {
       public Task<PaymentResult> CreatePaymentAsync(int orderId) { }
       public Task<PaymentResult> VerifyPaymentAsync(string payload, string signature) { }
   }
   ```

2. **Register Service**
   ```csharp
   // Choose provider based on configuration
   services.AddScoped<IPaymentService>(sp =>
   {
       var provider = configuration["Payment:Provider"];
       return provider switch
       {
           "Stripe" => new StripePaymentService(...),
           "PayPal" => new PayPalPaymentService(...),
           _ => throw new Exception("Unknown provider")
       };
   });
   ```

3. **Add Webhook Route**
   ```csharp
   [HttpPost("paypal")]
   public async Task<IActionResult> PayPalWebhook() { }
   ```

## Troubleshooting

### Issue: Webhook not received

**Solutions:**
1. Check Stripe Dashboard → Developers → Webhooks → Events
2. Verify endpoint URL is correct and accessible
3. Check webhook secret matches configuration
4. For local testing, ensure Stripe CLI is running

### Issue: Payment succeeds but order stays Pending

**Possible Causes:**
1. Webhook signature verification failing
2. Order ID not in webhook metadata
3. Database save failing

**Debug:**
```csharp
// Add logging in StripePaymentService.VerifyPaymentAsync()
_logger.LogInformation("Processing webhook for Order {OrderId}", orderId);
```

### Issue: "Order not eligible for payment"

**Causes:**
- Order Status is not Pending
- Order already has a transaction ID

**Check:**
```sql
SELECT Id, OrderNumber, Status, PaymentTransactionId 
FROM Orders 
WHERE Id = X
```

## Security Best Practices

1. **Never trust client-side data for payment confirmation**
2. **Always verify webhook signatures**
3. **Use HTTPS in production**
4. **Store API keys securely (Azure Key Vault, AWS Secrets Manager)**
5. **Implement rate limiting on webhook endpoint**
6. **Log all payment operations for audit**
7. **Handle PCI compliance (Stripe handles card data)**

## Monitoring & Logging

### Key Metrics to Track

- Payment success rate
- Webhook processing latency
- Failed payment reasons
- Duplicate webhook frequency

### Important Logs

```csharp
// Success
_logger.LogInformation("Order {OrderId} marked as Paid with transaction {TransactionId}");

// Failure
_logger.LogWarning("Payment creation failed for Order {OrderId}: {Error}");

// Security
_logger.LogWarning("Webhook signature verification failed");
```

## Production Readiness

### Before Going Live

- [ ] Replace test API keys with live keys
- [ ] Configure production webhook endpoint
- [ ] Enable webhook signature verification
- [ ] Set up monitoring and alerts
- [ ] Test failure scenarios
- [ ] Document refund process
- [ ] Set up Stripe Dashboard access for team
- [ ] Enable Stripe Radar for fraud prevention
- [ ] Configure email notifications
- [ ] Test high-volume scenarios

## Support

For Stripe-specific issues:
- Stripe Documentation: https://stripe.com/docs
- Stripe Support: https://support.stripe.com
- Stripe API Reference: https://stripe.com/docs/api

For implementation issues:
- Check logs in: `ElleganzaPlatform/logs/`
- Review webhook events in Stripe Dashboard
- Test with Stripe CLI for debugging
