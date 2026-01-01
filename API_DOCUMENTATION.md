# Drawing Marketplace API Documentation

## Base URL
```
http://localhost:8080/api
```

## Authentication

Hầu hết các endpoints yêu cầu JWT Bearer Token trong header:
```
Authorization: Bearer <access_token>
```

Token được lấy từ endpoint `/api/auth/login` hoặc `/api/auth/refresh`.

---

## Response Format

Tất cả API responses đều có format thống nhất:

### Success Response
```json
{
  "message": "Thành công",
  "message_en": "Success",
  "data": { ... },
  "status": "success",
  "timeStamp": "2025-01-15 10:30:00"
}
```

### Error Response
```json
{
  "message": "Lỗi xảy ra",
  "message_en": "Error occurred",
  "data": null,
  "status": "error",
  "timeStamp": "2025-01-15 10:30:00",
  "violations": [
    {
      "message": {
        "en": "Error message in English",
        "vi": "Thông báo lỗi bằng tiếng Việt"
      },
      "type": "ErrorType",
      "code": 400,
      "field": "fieldName"
    }
  ]
}
```

### HTTP Status Codes
- `200` - Success
- `201` - Created
- `202` - Accepted
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `500` - Internal Server Error

---

## 1. Authentication (`/api/auth`)

### 1.1 Register
**POST** `/api/auth/register`

**Authorization:** Not required

**Request Body:**
```json
{
  "email": "user@example.com",
  "username": "username123",
  "password": "Password123!"
}
```

**Validation:**
- `email`: Required, valid email format, max 100 characters
- `username`: Required, 3-50 characters, alphanumeric + underscore + hyphen
- `password`: Required, 8-100 characters, must contain: uppercase, lowercase, number, special character

**Response:** `202 Accepted`
```json
{
  "message": "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản",
  "message_en": "Registration successful. Please check your email to verify your account",
  "data": null,
  "status": "success"
}
```

---

### 1.2 Login
**POST** `/api/auth/login`

**Authorization:** Not required

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response:** `200 OK`
```json
{
  "message": "Đăng nhập thành công",
  "message_en": "Login successfully",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_string",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  },
  "status": "success"
}
```

---

### 1.3 Refresh Token
**POST** `/api/auth/refresh`

**Authorization:** Not required

**Request Body:**
```json
{
  "refreshToken": "refresh_token_string"
}
```

**Response:** `200 OK` (same as Login)

---

### 1.4 Logout
**POST** `/api/auth/logout`

**Authorization:** Required

**Request Body:**
```json
{
  "refreshToken": "refresh_token_string"
}
```

**Response:** `200 OK`

---

### 1.5 Logout All Devices
**POST** `/api/auth/logout-all`

**Authorization:** Required

**Response:** `200 OK`

---

### 1.6 Verify OTP (Account Verification)
**POST** `/api/auth/verify-otp`

**Authorization:** Not required

**Request Body:**
```json
{
  "email": "user@example.com",
  "otp": "123456"
}
```

**Response:** `200 OK`

---

### 1.7 Resend OTP
**POST** `/api/auth/resend-otp`

**Authorization:** Not required

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response:** `202 Accepted`

---

### 1.8 Forgot Password
**POST** `/api/auth/forgot-password`

**Authorization:** Not required

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response:** `202 Accepted`

---

### 1.9 Verify Reset Password OTP
**POST** `/api/auth/verify-reset-otp`

**Authorization:** Not required

**Request Body:**
```json
{
  "email": "user@example.com",
  "otp": "123456"
}
```

**Response:** `200 OK`

---

### 1.10 Reset Password
**POST** `/api/auth/reset-password`

**Authorization:** Not required

**Request Body:**
```json
{
  "email": "user@example.com",
  "newPassword": "NewPassword123!"
}
```

**Response:** `200 OK`

---

### 1.11 Get Profile
**GET** `/api/auth/profile`

**Authorization:** Required

**Response:** `200 OK`
```json
{
  "message": "Lấy thông tin profile thành công",
  "message_en": "Get profile successfully",
  "data": {
    "id": "guid",
    "email": "user@example.com",
    "role": "user"
  },
  "status": "success"
}
```

---

## 2. Banners (`/api/Banners`)

### 2.1 Get Active Banners
**GET** `/api/Banners`

**Authorization:** Not required

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "guid",
      "title": "Banner Title",
      "subtitle": "Banner Subtitle",
      "imageUrl": "https://...",
      "button1Text": "Button 1",
      "button1Link": "/link1",
      "button2Text": "Button 2",
      "button2Link": "/link2",
      "isActive": true,
      "displayOrder": 1,
      "createdAt": "2025-01-15T10:30:00Z"
    }
  ]
}
```

---

### 2.2 Get All Banners (Admin)
**GET** `/api/Banners/all`

**Authorization:** Required (Admin only)

**Response:** `200 OK` (same structure as 2.1)

---

### 2.3 Create Banner
**POST** `/api/Banners`

**Authorization:** Required (Admin only)

**Request:** `multipart/form-data`
```
title: string (required)
subtitle: string (optional)
image: file (required)
button1Text: string (optional)
button1Link: string (optional)
button2Text: string (optional)
button2Link: string (optional)
isActive: boolean (optional, default: true)
displayOrder: integer (optional, default: 0)
```

**Response:** `201 Created`

---

### 2.4 Update Banner
**PUT** `/api/Banners/{id}`

**Authorization:** Required (Admin only)

**Request:** `multipart/form-data` (same fields as Create, all optional)

**Response:** `200 OK`

---

### 2.5 Delete Banner
**DELETE** `/api/Banners/{id}`

**Authorization:** Required (Admin only)

**Response:** `200 OK`

---

### 2.6 Get Banner By ID
**GET** `/api/Banners/{id}`

**Authorization:** Required (Admin only)

**Response:** `200 OK` (single banner object)

---

## 3. Categories (`/api/categories`)

### 3.1 Get All Categories
**GET** `/api/categories`

**Authorization:** Not required

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "guid",
      "name": "Category Name",
      "parentId": "guid or null",
      "createdAt": "2025-01-15T10:30:00Z"
    }
  ]
}
```

---

### 3.2 Get Category By ID
**GET** `/api/categories/{id}`

**Authorization:** Not required

**Response:** `200 OK` (single category object)

---

### 3.3 Create Category
**POST** `/api/categories`

**Authorization:** Required (Admin only)

**Request Body:**
```json
{
  "name": "Category Name",
  "parentId": "guid or null"
}
```

**Response:** `201 Created`

---

### 3.4 Update Category
**PUT** `/api/categories/{id}`

**Authorization:** Required (Admin only)

**Request Body:**
```json
{
  "name": "Updated Category Name",
  "parentId": "guid or null"
}
```

**Response:** `200 OK`

---

### 3.5 Delete Category
**DELETE** `/api/categories/{id}`

**Authorization:** Required (Admin only)

**Response:** `200 OK`

---

## 4. Contents (`/api/contents`)

### 4.1 Get Public Contents (Paginated)
**GET** `/api/contents`

**Authorization:** Not required

**Query Parameters:**
- `page`: integer (default: 1)
- `pageSize`: integer (default: 10)
- `keyword`: string (optional, search in title/description)
- `categoryName`: string (optional)
- `minPrice`: decimal (optional)
- `maxPrice`: decimal (optional)
- `sortBy`: enum (Newest, Oldest, PriceAsc, PriceDesc, Popular) (default: Newest)
- `sortDir`: enum (Asc, Desc) (default: Desc)

**Response:** `200 OK`
```json
{
  "data": {
    "items": [
      {
        "id": "guid",
        "title": "Content Title",
        "description": "Description",
        "price": 100000,
        "thumbnailUrl": "https://...",
        "categoryName": "Category",
        "status": "published",
        "createdAt": "2025-01-15T10:30:00Z"
      }
    ],
    "totalCount": 100,
    "page": 1,
    "pageSize": 10,
    "totalPages": 10
  }
}
```

---

### 4.2 Get Management Contents (Paginated)
**GET** `/api/contents/management`

**Authorization:** Required

**Query Parameters:** (same as 4.1, plus)
- `status`: enum (draft, published, archived) (optional)
- `collaboratorId`: guid (optional)

**Response:** `200 OK` (same structure as 4.1)

---

### 4.3 Get Content By ID (Public)
**GET** `/api/contents/{id}`

**Authorization:** Not required

**Response:** `200 OK`
```json
{
  "data": {
    "id": "guid",
    "title": "Content Title",
    "description": "Description",
    "price": 100000,
    "thumbnailUrl": "https://...",
    "previewUrls": ["https://...", "https://..."],
    "categoryName": "Category",
    "status": "published",
    "createdAt": "2025-01-15T10:30:00Z"
  }
}
```

---

### 4.4 Get Content By ID (Management)
**GET** `/api/contents/management/{id}`

**Authorization:** Required

**Response:** `200 OK` (includes downloadable files and more details)

---

### 4.5 Create Content
**POST** `/api/contents`

**Authorization:** Required

**Request:** `multipart/form-data`
```
title: string (required)
description: string (optional)
price: decimal (required, >= 0)
categoryId: guid (optional)
collaboratorId: guid (optional)
thumbnailFile: file (required)
previewFiles: file[] (optional, multiple files)
downloadableFiles: file[] (optional, multiple files)
```

**Response:** `201 Created`

---

### 4.6 Update Content
**PUT** `/api/contents/{id}`

**Authorization:** Required

**Request:** `multipart/form-data`
```
title: string (required)
description: string (optional)
price: decimal (required, >= 0)
categoryId: guid (optional)
thumbnailFile: file (optional)
previewFiles: file[] (optional)
downloadableFiles: file[] (optional)
filesToDelete: guid[] (optional, array of file IDs to delete)
```

**Response:** `200 OK`

---

### 4.7 Delete Content
**DELETE** `/api/contents/{id}`

**Authorization:** Required

**Response:** `200 OK`

---

### 4.8 Update Content Status (Admin)
**PATCH** `/api/contents/{id}/status`

**Authorization:** Required (Admin only)

**Request Body:**
```json
true  // to publish
false // to archive
```

**Response:** `200 OK`

---

## 5. Cart (`/api/cart`)

### 5.1 Get Cart
**GET** `/api/cart`

**Authorization:** Required

**Response:** `200 OK`
```json
{
  "data": {
    "items": [
      {
        "contentId": "guid",
        "title": "Content Title",
        "price": 100000,
        "imageUrl": "https://...",
        "createdAt": "2025-01-15T10:30:00Z"
      }
    ],
    "totalAmount": 200000,
    "itemCount": 2
  }
}
```

---

### 5.2 Add to Cart
**POST** `/api/cart`

**Authorization:** Required

**Request Body:**
```json
{
  "contentId": "guid"
}
```

**Response:** `200 OK` (updated cart)

---

### 5.3 Remove from Cart
**DELETE** `/api/cart/{contentId}`

**Authorization:** Required

**Response:** `200 OK` (updated cart)

---

### 5.4 Clear Cart
**DELETE** `/api/cart/clear`

**Authorization:** Required

**Response:** `200 OK` (empty cart)

---

## 6. Orders (`/api/orders`)

### 6.1 Create Order
**POST** `/api/orders`

**Authorization:** Required

**Request Body:**
```json
{
  "couponCode": "DISCOUNT10" (optional),
  "paymentMethod": "vnpay" (required, "vnpay" or "momo")
}
```

**Response:** `201 Created`
```json
{
  "data": {
    "id": "guid",
    "userId": "guid",
    "totalAmount": 200000,
    "currency": "VND",
    "status": "pending",
    "createdAt": "2025-01-15T10:30:00Z",
    "items": [
      {
        "contentId": "guid",
        "contentTitle": "Content Title",
        "collaboratorId": "guid",
        "price": 100000
      }
    ],
    "payment": {
      "id": "guid",
      "amount": 200000,
      "paymentMethod": "vnpay",
      "status": "pending",
      "paymentUrl": "https://sandbox.vnpayment.vn/...",
      "createdAt": "2025-01-15T10:30:00Z"
    },
    "coupon": {
      "code": "DISCOUNT10",
      "discountAmount": 20000
    }
  }
}
```

**Note:** User should redirect to `payment.paymentUrl` to complete payment.

---

### 6.2 Get My Orders
**GET** `/api/orders`

**Authorization:** Required

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "guid",
      "totalAmount": 200000,
      "status": "paid",
      "createdAt": "2025-01-15T10:30:00Z",
      "items": [...],
      "payment": {...}
    }
  ]
}
```

---

### 6.3 Get Order By ID
**GET** `/api/orders/{id}`

**Authorization:** Required

**Response:** `200 OK` (single order object, same structure as 6.1)

---

### 6.4 Cancel Order
**POST** `/api/orders/{id}/cancel`

**Authorization:** Required

**Response:** `200 OK`

**Note:** If order is paid, refund will be processed and commission will be rolled back.

---

## 7. Payment Callbacks (`/api/payments`)

### 7.1 VNPay Callback
**GET/POST** `/api/payments/vnpay/callback`

**Authorization:** Not required (called by VNPay)

**Query Parameters (VNPay sends):**
- `vnp_TxnRef`: Order ID
- `vnp_ResponseCode`: "00" = success, others = failed
- `vnp_TransactionNo`: Transaction ID

**Response:** `200 OK`
```json
{
  "code": 200,
  "message": "Success"
}
```

---

### 7.2 MoMo Callback
**POST** `/api/payments/momo/callback`

**Authorization:** Not required (called by MoMo)

**Request Body (MoMo sends):**
```json
{
  "orderId": "guid",
  "resultCode": "0" (0 = success, others = failed),
  "transId": "transaction_id"
}
```

**Response:** `200 OK`
```json
{
  "code": 0,
  "message": "Success"
}
```

---

## 8. Coupons (`/api/coupons`)

### 8.1 Get All Coupons
**GET** `/api/coupons`

**Authorization:** Not required

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "guid",
      "code": "DISCOUNT10",
      "type": "percent" (or "fixed_amount"),
      "value": 10,
      "maxDiscount": 50000,
      "minOrderAmount": 100000,
      "usageLimit": 100,
      "usedCount": 50,
      "validFrom": "2025-01-01T00:00:00Z",
      "validTo": "2025-12-31T23:59:59Z",
      "isActive": true
    }
  ]
}
```

---

### 8.2 Get Coupon By Code
**GET** `/api/coupons/{code}`

**Authorization:** Not required

**Response:** `200 OK` (single coupon object)

---

### 8.3 Create Coupon
**POST** `/api/coupons`

**Authorization:** Required (Admin only)

**Request Body:**
```json
{
  "code": "DISCOUNT10" (required),
  "type": "percent" (required, "percent" or "fixed_amount"),
  "value": 10 (required, > 0),
  "maxDiscount": 50000 (optional),
  "minOrderAmount": 100000 (optional),
  "usageLimit": 100 (optional),
  "validFrom": "2025-01-01T00:00:00Z" (optional),
  "validTo": "2025-12-31T23:59:59Z" (optional),
  "isActive": true (optional, default: true)
}
```

**Response:** `201 Created`

---

### 8.4 Update Coupon
**PUT** `/api/coupons/{id}`

**Authorization:** Required (Admin only)

**Request Body:** (all fields optional)
```json
{
  "type": "percent",
  "value": 15,
  "maxDiscount": 60000,
  "minOrderAmount": 150000,
  "usageLimit": 200,
  "validFrom": "2025-01-01T00:00:00Z",
  "validTo": "2025-12-31T23:59:59Z",
  "isActive": false
}
```

**Response:** `200 OK`

---

### 8.5 Delete Coupon
**DELETE** `/api/coupons/{id}`

**Authorization:** Required (Admin only)

**Response:** `200 OK`

---

## 9. Collaborators (`/api/collaborators`)

### 9.1 Apply as Collaborator
**POST** `/api/collaborators`

**Authorization:** Required

**Response:** `202 Accepted`

---

### 9.2 Update Collaborator Status (Admin)
**PATCH** `/api/collaborators/{id}`

**Authorization:** Required (Admin only)

**Request Body:**
```json
"approved"  // to approve
"rejected"  // to reject
```

**Response:** `200 OK`

---

## 10. Wallets (`/api/wallets`)

### 10.1 Get My Wallet
**GET** `/api/wallets/my`

**Authorization:** Required

**Response:** `200 OK`
```json
{
  "data": {
    "id": "guid",
    "ownerType": "user",
    "ownerId": "guid",
    "balance": 1000000,
    "updatedAt": "2025-01-15T10:30:00Z"
  }
}
```

---

### 10.2 Get My Transactions
**GET** `/api/wallets/my/transactions`

**Authorization:** Required

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "guid",
      "type": "credit" (or "debit", "commission", "withdrawal"),
      "amount": 100000,
      "referenceId": "guid",
      "createdAt": "2025-01-15T10:30:00Z"
    }
  ]
}
```

---

### 10.3 Get Collaborator Stats
**GET** `/api/wallets/collaborator/stats`

**Authorization:** Required (Collaborator role)

**Response:** `200 OK`
```json
{
  "data": {
    "totalEarnings": 5000000,
    "totalWithdrawn": 2000000,
    "pendingWithdrawals": 500000,
    "availableBalance": 2500000,
    "totalCommission": 5000000,
    "transactionCount": 50
  }
}
```

---

### 10.4 Get Collaborator Transactions
**GET** `/api/wallets/collaborator/transactions`

**Authorization:** Required (Collaborator role)

**Response:** `200 OK` (same structure as 10.2)

---

## 11. Withdrawals (`/api/withdrawals`)

### 11.1 Create Withdrawal Request
**POST** `/api/withdrawals`

**Authorization:** Required (Collaborator role)

**Request Body:**
```json
{
  "bankId": "guid" (required),
  "amount": 500000 (required, minimum: 500000)
}
```

**Response:** `201 Created`
```json
{
  "data": {
    "id": "guid",
    "collaboratorId": "guid",
    "bankId": "guid",
    "amount": 500000,
    "status": "pending",
    "createdAt": "2025-01-15T10:30:00Z"
  }
}
```

---

### 11.2 Get My Withdrawals
**GET** `/api/withdrawals/my`

**Authorization:** Required (Collaborator role)

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "guid",
      "amount": 500000,
      "status": "pending" (or "approved", "rejected", "paid"),
      "createdAt": "2025-01-15T10:30:00Z",
      "processedAt": "2025-01-16T10:30:00Z" (if processed),
      "processedBy": "guid" (if processed)
    }
  ]
}
```

---

### 11.3 Get Pending Withdrawals (Admin)
**GET** `/api/withdrawals/pending`

**Authorization:** Required (Admin only)

**Response:** `200 OK` (array of withdrawal objects)

---

### 11.4 Approve Withdrawal (Admin)
**POST** `/api/withdrawals/{id}/approve`

**Authorization:** Required (Admin only)

**Response:** `200 OK`

**Note:** 
- If amount >= 2,000,000 VND, 10% personal income tax will be deducted
- Transfer fee will be deducted
- Wallet balance will be decreased

---

### 11.5 Reject Withdrawal (Admin)
**POST** `/api/withdrawals/{id}/reject`

**Authorization:** Required (Admin only)

**Response:** `200 OK`

---

## 12. Statistics (`/api/statistics`)

### 12.1 Get Wallet Statistics
**GET** `/api/statistics/wallet`

**Authorization:** Required (Collaborator role)

**Response:** `200 OK` (same structure as 10.3)

---

## Error Handling

### Common Error Responses

#### 400 Bad Request
```json
{
  "message": "Dữ liệu không hợp lệ",
  "message_en": "Invalid data",
  "status": "fail",
  "violations": [
    {
      "message": {
        "en": "Email is required",
        "vi": "Email là bắt buộc"
      },
      "type": "ValidationError",
      "code": 400,
      "field": "email"
    }
  ]
}
```

#### 401 Unauthorized
```json
{
  "message": "Tên đăng nhập hoặc mật khẩu không chính xác",
  "message_en": "Username or password is incorrect",
  "status": "fail",
  "violations": [
    {
      "message": {
        "en": "Username or password is incorrect",
        "vi": "Tên đăng nhập hoặc mật khẩu không chính xác"
      },
      "type": "InvalidCredentials",
      "code": 401
    }
  ]
}
```

#### 403 Forbidden
```json
{
  "message": "Tài khoản chưa được kích hoạt",
  "message_en": "Account is not activated",
  "status": "fail",
  "violations": [
    {
      "message": {
        "en": "Account is not activated",
        "vi": "Tài khoản chưa được kích hoạt"
      },
      "type": "AccountNotActive",
      "code": 403
    }
  ]
}
```

#### 404 Not Found
```json
{
  "message": "Resource not found",
  "message_en": "Resource not found",
  "status": "error",
  "data": null
}
```

---

## Notes

1. **File Uploads**: Use `multipart/form-data` for endpoints that accept files (Banners, Contents)
2. **Pagination**: All paginated endpoints return `PagedResultDto` with `items`, `totalCount`, `page`, `pageSize`, `totalPages`
3. **Payment Flow**:
   - Create order → Get `paymentUrl` from response
   - Redirect user to `paymentUrl`
   - Payment gateway will callback to `/api/payments/{gateway}/callback`
   - After successful payment, commission (10%) is automatically added to collaborator wallet
4. **Commission**: 10% of content price is added to collaborator wallet when order is paid successfully
5. **Withdrawal**: Minimum 500,000 VND, 10% tax if >= 2,000,000 VND
6. **Roles**: `user`, `collaborator`, `admin`
7. **Content Status**: `draft`, `published`, `archived`
8. **Order Status**: `pending`, `paid`, `cancelled`, `failed`
9. **Payment Status**: `pending`, `success`, `failed`
10. **Withdrawal Status**: `pending`, `approved`, `rejected`, `paid`

---

## Example Usage

### Complete Order Flow

1. **Add items to cart**
   ```
   POST /api/cart
   { "contentId": "..." }
   ```

2. **Create order**
   ```
   POST /api/orders
   { "paymentMethod": "vnpay", "couponCode": "DISCOUNT10" }
   ```

3. **Redirect user to payment URL** (from response `data.payment.paymentUrl`)

4. **Payment gateway redirects back** to your frontend

5. **Check order status**
   ```
   GET /api/orders/{orderId}
   ```

---

**Last Updated:** 2025-01-15

