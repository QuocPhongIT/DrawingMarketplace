# Drawing Marketplace - API Documentation

**Cập nhật lần cuối:** 2025-01-15  
**Phiên bản API:** 1.0.0  
**Mô tả:** Tài liệu API đầy đủ và chính xác cho Drawing Marketplace

---

## Table of Contents

1. [Authentication API](#authentication-api)
2. [Content API](#content-api)
3. [Cart API](#cart-api)
4. [Order API](#order-api)
5. [Wallet API](#wallet-api)
6. [Withdrawal API](#withdrawal-api)
7. [Review API](#review-api)
8. [Banner API](#banner-api)
9. [Category API](#category-api)
10. [Coupon API](#coupon-api)
11. [Copyright Report API](#copyright-report-api)
12. [Download API](#download-api)
13. [Collaborator API](#collaborator-api)
14. [Content Statistics API](#content-statistics-api)

---

## Authentication API

### POST /api/auth/register
Đăng ký tài khoản mới

**Request Body:**
```json
{
  "username": "user123",
  "email": "user@example.com",
  "password": "Password@123",
}
```

**Response:** `201 Created`
```json
{
  "message": "Đăng ký thành công",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  },
  "status": "success"
}
```

**Error Responses:**
- `400 Bad Request` - Email already exists or password requirements not met
- `400 Bad Request` - Invalid input

---

### POST /api/auth/login
Đăng nhập

**Request Body:**
```json
{
  "username": "user123",
  "password": "Password@123"
}
```

**Response:** `200 OK`
```json
{
  "message": "Đăng nhập thành công",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  },
  "status": "success"
}
```

**Lưu ý:** Response chỉ bao gồm token information, KHÔNG chứa user data (id, email, etc.)

---

### POST /api/auth/logout
Đăng xuất

**Headers:**
```
Authorization: Bearer <access_token>
```

**Response:** `200 OK`
```json
{
  "message": "Đăng xuất thành công",
  "data": null,
  "status": "success"
}
```

---

### POST /api/auth/logout-all
Đăng xuất tất cả phiên đăng nhập

**Headers:**
```
Authorization: Bearer <access_token>
```

**Response:** `200 OK`
```json
{
  "message": "Đăng xuất tất cả phiên thành công",
  "data": null,
  "status": "success"
}
```

---

### POST /api/auth/refresh
Làm mới token

**Request Body:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Response:** `200 OK`
```json
{
  "message": "Làm mới token thành công",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  },
  "status": "success"
}
```

---

### POST /api/auth/verify-email
Xác minh email

**Request Body:**
```json
{
  "email": "user@example.com",
  "otp": "123456"
}
```

**Response:** `200 OK`
```json
{
  "message": "Xác minh email thành công",
  "data": null,
  "status": "success"
}
```

---

### POST /api/auth/resend-otp
Gửi lại OTP

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response:** `200 OK`
```json
{
  "message": "Gửi OTP thành công",
  "data": null,
  "status": "success"
}
```

---

### POST /api/auth/forgot-password
Quên mật khẩu

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response:** `200 OK`
```json
{
  "message": "Gửi email reset mật khẩu thành công",
  "data": null,
  "status": "success"
}
```

---

### POST /api/auth/reset-password
Đặt lại mật khẩu

**Request Body:**
```json
{
  "email": "user@example.com",
  "resetToken": "token...",
  "newPassword": "NewPassword@123"
}
```

**Response:** `200 OK`
```json
{
  "message": "Đặt lại mật khẩu thành công",
  "data": null,
  "status": "success"
}
```

---

### GET /api/auth/profile
Lấy thông tin profile người dùng

**Authentication:** Required

**Headers:**
```
Authorization: Bearer <access_token>
```

**Response:** `200 OK`
```json
{
  "message": "Lấy thông tin profile thành công",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440100",
    "username": "john_doe",
    "email": "john@example.com",
    "status": "Active",
    "createdAt": "15/01/2026 10:00",
    "updatedAt": "15/01/2026 14:30",
    "roleDisplay": "Cộng tác viên",
    "isCollaborator": true,
    "collaboratorStatus": "Active"
  },
  "status": "success"
}
```

**Fields:**
- `id` - ID người dùng
- `username` - Tên đăng nhập
- `email` - Email đăng ký
- `status` - Trạng thái tài khoản (Active/Inactive)
- `createdAt` - Ngày tạo tài khoản (định dạng dd/MM/yyyy HH:mm)
- `updatedAt` - Ngày cập nhật gần nhất (định dạng dd/MM/yyyy HH:mm)
- `roleDisplay` - Vai trò hiển thị (Cộng tác viên/Người dùng/Admin)
- `isCollaborator` - Có phải CTV hay không
- `collaboratorStatus` - Trạng thái CTV (nếu là CTV)

**Error Responses:**
- `401 Unauthorized` - Token không hợp lệ
- `404 Not Found` - Không tìm thấy thông tin người dùng

---

## Content API

### GET /api/contents
Lấy danh sách content công khai (Phân trang)

**Query Parameters:**
```
page=1
pageSize=10
keyword=landscape
categoryName=Art
minPrice=0
maxPrice=500
sortBy=Newest (Newest, Oldest, Cheapest, MostExpensive, MostViewed, MostPurchased, MostDownloaded, TrendingNow)
sortDir=Desc (Desc, Asc)
```

**Response:** `200 OK`
```json
{
  "message": "Lấy danh sách content thành công",
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "title": "Beautiful Landscape",
        "description": "A stunning landscape painting...",
        "price": 99.99,
        "status": "Published",
        "categoryId": "550e8400-e29b-41d4-a716-446655440001",
        "categoryName": "Art",
        "collaboratorId": "550e8400-e29b-41d4-a716-446655440002",
        "collaboratorUsername": "artist_pro",
        "createdAt": "2025-01-10T10:00:00Z",
        "updatedAt": "2025-01-15T14:30:00Z",
        "thumbnailUrl": "https://...",
        "views": 150,
        "purchases": 5,
        "downloads": 10,
        "totalRevenue": 499.95,
        "conversionRate": 3.33
      }
    ],
    "totalCount": 45,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5
  },
  "status": "success"
}
```

---

### GET /api/contents/management
Lấy danh sách content (Quản trị - Phân trang)

**Authentication:** Required (Bearer Token)

**Query Parameters:**
```
page=1
pageSize=10
keyword=landscape
categoryName=Art
status=Draft (Draft, Published, Rejected, Archived)
collaboratorId=550e8400-e29b-41d4-a716-446655440000
sortBy=Newest
sortDir=Desc
```

**Response:** `200 OK`
```json
{
  "message": "Lấy danh sách content quản trị thành công",
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "title": "Beautiful Landscape",
        "description": "A stunning landscape painting...",
        "price": 99.99,
        "status": "Draft",
        "categoryId": "550e8400-e29b-41d4-a716-446655440001",
        "collaboratorId": "550e8400-e29b-41d4-a716-446655440002",
        "createdAt": "2025-01-10T10:00:00Z",
        "updatedAt": "2025-01-15T14:30:00Z",
        "thumbnailUrl": "https://...",
        "views": 0,
        "purchases": 0,
        "downloads": 0,
        "totalRevenue": 0,
        "conversionRate": 0
      }
    ],
    "totalCount": 12,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 2
  },
  "status": "success"
}
```

---

### GET /api/contents/{id}
Lấy chi tiết content công khai

**Response:** `200 OK`
```json
{
  "message": "Lấy chi tiết content thành công",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "title": "Beautiful Landscape",
    "description": "A stunning landscape painting...",
    "price": 99.99,
    "status": "Published",
    "categoryId": "550e8400-e29b-41d4-a716-446655440001",
    "collaboratorId": "550e8400-e29b-41d4-a716-446655440002",
    "createdAt": "2025-01-10T10:00:00Z",
    "updatedAt": "2025-01-15T14:30:00Z",
    "thumbnailUrl": "https://...",
    "stats": {
      "views": 150,
      "downloads": 10,
      "purchases": 5
    },
    "previewFiles": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440010",
        "fileName": "preview.jpg",
        "fileUrl": "https://...",
        "fileType": "image/jpeg",
        "size": 256000,
        "purpose": "Preview",
        "displayOrder": 1,
        "createdAt": "2025-01-10T10:00:00Z"
      }
    ],
    "downloadableFiles": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440011",
        "fileName": "landscape_hd.psd",
        "fileUrl": "https://...",
        "fileType": "application/x-psd",
        "size": 5242880,
        "purpose": "Downloadable",
        "displayOrder": 1,
        "createdAt": "2025-01-10T10:00:00Z"
      }
    ]
  },
  "status": "success"
}
```

---

### GET /api/contents/management/{id}
Lấy chi tiết content (Quản trị)

**Authentication:** Required

**Response:** `200 OK`  
(Cấu trúc tương tự GET /api/contents/{id})

---

### POST /api/contents
Tạo mới content

**Authentication:** Required

**Request Body:** `multipart/form-data`
```
title: "Beautiful Landscape"
description: "A stunning landscape painting..."
price: 99.99
categoryId: "550e8400-e29b-41d4-a716-446655440001"
files: [File, File, ...] (Thumbnail + Preview + Downloadable files)
```

**Response:** `201 Created`
```json
{
  "message": "Tạo content thành công",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "title": "Beautiful Landscape",
    "description": "A stunning landscape painting...",
    "price": 99.99,
    "status": "Draft",
    "categoryId": "550e8400-e29b-41d4-a716-446655440001",
    "collaboratorId": "550e8400-e29b-41d4-a716-446655440002",
    "createdAt": "2025-01-15T10:00:00Z",
    "updatedAt": null,
    "thumbnailUrl": "https://...",
    "stats": null,
    "previewFiles": [],
    "downloadableFiles": []
  },
  "status": "success"
}
```

---

### PUT /api/contents/{id}
Cập nhật content

**Authentication:** Required

**Request Body:** `multipart/form-data`
```
title: "Updated Title"
description: "Updated description..."
price: 109.99
categoryId: "550e8400-e29b-41d4-a716-446655440001"
files: [File, ...] (Optional)
```

**Response:** `200 OK`  
(Cấu trúc tương tự POST /api/contents)

---

### DELETE /api/contents/{id}
Xóa content

**Authentication:** Required

**Response:** `200 OK`
```json
{
  "message": "Xóa content thành công",
  "data": null,
  "status": "success"
}
```

---

### PATCH /api/contents/{id}/status
Phê duyệt hoặc từ chối content

**Authentication:** Required (Admin role)

**Request Body:**
```json
{
  "publish": true
}
```

**Response:** `200 OK`
```json
{
  "message": "Phê duyệt content thành công",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "title": "Beautiful Landscape",
    "status": "Published",
    ...
  },
  "status": "success"
}
```

---

## Cart API

### GET /api/cart
Lấy giỏ hàng hiện tại

**Authentication:** Required

**Response:** `200 OK`
```json
{
  "message": "Lấy giỏ hàng thành công",
  "data": {
    "items": [
      {
        "contentId": "550e8400-e29b-41d4-a716-446655440000",
        "title": "Beautiful Landscape",
        "imageUrl": "https://...",
        "price": 99.99,
        "quantity": 1,
        "subtotal": 99.99
      },
      {
        "contentId": "550e8400-e29b-41d4-a716-446655440001",
        "title": "Modern Design",
        "imageUrl": "https://...",
        "price": 149.99,
        "quantity": 2,
        "subtotal": 299.98
      }
    ],
    "totalAmount": 399.97,
    "itemCount": 2,
    "currency": "VND"
  },
  "status": "success"
}
```

---

### POST /api/cart
Thêm sản phẩm vào giỏ hàng

**Request Body:**
```json
{
  "contentId": "550e8400-e29b-41d4-a716-446655440000",
  "quantity": 1
}
```

**Response:** `200 OK`  
(Cấu trúc tương tự GET /api/cart)

---

### DELETE /api/cart/{contentId}
Xóa sản phẩm khỏi giỏ hàng

**Response:** `200 OK`  
(Cấu trúc tương tự GET /api/cart)

---

### DELETE /api/cart/clear
Xóa toàn bộ giỏ hàng

**Response:** `200 OK`
```json
{
  "message": "Xóa toàn bộ giỏ hàng thành công",
  "data": {
    "items": [],
    "totalAmount": 0,
    "itemCount": 0,
    "currency": "VND"
  },
  "status": "success"
}
```

---

## Order API

### POST /api/orders
Tạo đơn hàng

**Authentication:** Required

**Request Body:**
```json
{
  "contentId": "550e8400-e29b-41d4-a716-446655440000",
  "quantity": 1,
  "couponCode": "SAVE10",
  "paymentMethod": "vnpay"
}
```

**Lưu ý:**
- Nếu `contentId` được cung cấp: Mua riêng content đó
- Nếu `contentId` không cung cấp: Mua từ giỏ hàng hiện tại

**Response:** `201 Created`
```json
{
  "message": "Tạo đơn hàng thành công",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440050",
    "userId": "550e8400-e29b-41d4-a716-446655440100",
    "subtotal": 399.97,
    "discountAmount": 40.00,
    "totalAmount": 359.97,
    "currency": "VND",
    "status": "Pending",
    "createdAt": "2025-01-15T10:00:00Z",
    "paidAt": null,
    "items": [
      {
        "contentId": "550e8400-e29b-41d4-a716-446655440000",
        "contentTitle": "Beautiful Landscape",
        "imageUrl": "https://...",
        "quantity": 1,
        "unitPrice": 99.99,
        "subtotal": 99.99
      },
      {
        "contentId": "550e8400-e29b-41d4-a716-446655440001",
        "contentTitle": "Modern Design",
        "imageUrl": "https://...",
        "quantity": 2,
        "unitPrice": 149.99,
        "subtotal": 299.98
      }
    ],
    "payment": {
      "id": "550e8400-e29b-41d4-a716-446655440200",
      "method": "vnpay",
      "transactionId": null,
      "amount": 359.97,
      "status": "Pending",
      "paidAt": null,
      "paymentUrl": "https://pay.vnpay.vn/...",
      "transactionNo": null
    },
    "coupon": {
      "code": "SAVE10",
      "discountValue": 10,
      "isPercentage": true,
      "maxDiscountAmount": null,
      "discountAmount": 40.00
    }
  },
  "status": "success"
}
```

---

### GET /api/orders
Lấy danh sách đơn hàng của tôi

**Authentication:** Required

**Response:** `200 OK`
```json
{
  "message": "Lấy danh sách đơn hàng thành công",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440050",
      "userId": "550e8400-e29b-41d4-a716-446655440100",
      "subtotal": 399.97,
      "discountAmount": 40.00,
      "totalAmount": 359.97,
      "currency": "VND",
      "status": "Pending",
      "createdAt": "2025-01-15T10:00:00Z",
      "paidAt": null,
      "items": [...],
      "payment": {...},
      "coupon": {...}
    }
  ],
  "status": "success"
}
```

---

### GET /api/orders/{id}
Lấy chi tiết đơn hàng

**Authentication:** Required

**Response:** `200 OK`  
(Cấu trúc tương tự POST /api/orders)

---

### POST /api/orders/{id}/cancel
Hủy đơn hàng

**Authentication:** Required

**Response:** `200 OK`
```json
{
  "message": "Hủy đơn hàng thành công",
  "data": null,
  "status": "success"
}
```

---

### GET /api/orders/vnpay/callback
Callback từ VNPay

**Query Parameters:**
```
vnp_Amount, vnp_BankCode, vnp_BankTmnCode, vnp_CommandId, vnp_CreateDate, 
vnp_CurrCode, vnp_OrderInfo, vnp_OrderType, vnp_PayDate, vnp_ResponseCode, 
vnp_SecureHash, vnp_SecureHashType, vnp_TmnCode, vnp_TransactionNo, vnp_TransactionStatus, 
vnp_TxnRef, vnp_Version
```

**Response:** `200 OK` (HTML Content)
```html
<html>
  <body>
    <h2>Thanh toán thành công</h2>
    <p>Mã giao dịch: 25011510000001</p>
  </body>
</html>
```

---

### GET /api/orders/vnpay/ipn
VNPay IPN Endpoint (Instant Payment Notification)

**Query Parameters:** (Tương tự callback)

**Response:** `200 OK`
```json
{
  "RspCode": "00",
  "Message": "Confirm Success"
}
```

---

### GET /api/orders/vnpay/fake-ipn
Fake IPN Endpoint (Dev/Test)

**Response:** `200 OK`  
(Cấu trúc tương tự /vnpay/ipn)

---

## Wallet API

### GET /api/wallets/my
Lấy số dư ví của tôi (CTV)

**Authentication:** Required (Collaborator role)

**Response:** `200 OK`
```json
{
  "message": "Lấy thông tin ví thành công",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440300",
    "balance": 5000.00,
    "updatedAt": "2025-01-15T14:30:00Z"
  },
  "status": "success"
}
```

---

### GET /api/wallets/my/transactions
Lấy lịch sử giao dịch ví của tôi

**Authentication:** Required (Collaborator role)

**Response:** `200 OK`
```json
{
  "message": "Lấy lịch sử giao dịch thành công",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440400",
      "type": "commission",
      "amount": 100.00,
      "referenceId": "550e8400-e29b-41d4-a716-446655440050",
      "createdAt": "2025-01-15T10:00:00Z"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440401",
      "type": "debit",
      "amount": -50.00,
      "referenceId": "550e8400-e29b-41d4-a716-446655440500",
      "createdAt": "2025-01-14T15:30:00Z"
    }
  ],
  "status": "success"
}
```

**Transaction Types:** `credit`, `debit`, `commission`, `withdrawal`

---

### GET /api/wallets/my/stats
Lấy thống kê ví của tôi

**Authentication:** Required (Collaborator role)

**Response:** `200 OK`
```json
{
  "message": "Lấy thống kê ví thành công",
  "data": {
    "totalBalance": 5000.00,
    "totalCommission": 15000.00,
    "totalWithdrawn": 10000.00,
    "pendingWithdrawal": 500.00
  },
  "status": "success"
}
```

---

## Withdrawal API

### POST /api/withdrawals
Tạo yêu cầu rút tiền

**Authentication:** Required (Collaborator role)

**Request Body:**
```json
{
  "bankId": "550e8400-e29b-41d4-a716-446655440600",
  "amount": 5000.00
}
```

**Response:** `201 Created`
```json
{
  "message": "Tạo yêu cầu rút tiền thành công",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440500",
    "collaboratorId": "550e8400-e29b-41d4-a716-446655440100",
    "bankId": "550e8400-e29b-41d4-a716-446655440600",
    "bankName": "Vietcombank",
    "bankAccount": "1234567890",
    "ownerName": "John Doe",
    "amount": 5000.00,
    "taxAmount": 50.00,
    "feeAmount": 10.00,
    "finalAmount": 4940.00,
    "status": "Pending",
    "createdAt": "2025-01-15T10:00:00Z",
    "processedAt": null
  },
  "status": "success"
}
```

---

### GET /api/withdrawals/my
Lấy danh sách yêu cầu rút tiền của tôi

**Authentication:** Required (Collaborator role)

**Response:** `200 OK`
```json
{
  "message": "Lấy danh sách yêu cầu rút tiền thành công",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440500",
      "collaboratorId": "550e8400-e29b-41d4-a716-446655440100",
      "bankId": "550e8400-e29b-41d4-a716-446655440600",
      "bankName": "Vietcombank",
      "bankAccount": "1234567890",
      "ownerName": "John Doe",
      "amount": 5000.00,
      "taxAmount": 50.00,
      "feeAmount": 10.00,
      "finalAmount": 4940.00,
      "status": "Pending",
      "createdAt": "2025-01-15T10:00:00Z",
      "processedAt": null
    }
  ],
  "status": "success"
}
```

---

### GET /api/withdrawals/pending
Lấy danh sách yêu cầu rút tiền chờ duyệt

**Authentication:** Required (Admin role)

**Response:** `200 OK`  
(Cấu trúc tương tự GET /api/withdrawals/my)

---

### GET /api/withdrawals
Lấy tất cả yêu cầu rút tiền

**Authentication:** Required (Admin role)

**Query Parameters:**
```
status=Pending (Pending, Approved, Rejected)
fromDate=2025-01-01T00:00:00Z
toDate=2025-01-31T23:59:59Z
page=1
pageSize=20
```

**Response:** `200 OK`
```json
{
  "message": "Lấy danh sách tất cả yêu cầu rút tiền thành công",
  "data": {
    "items": [...],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 3
  },
  "status": "success"
}
```

---

### POST /api/withdrawals/{id}/approve
Duyệt yêu cầu rút tiền

**Authentication:** Required (Admin role)

**Response:** `200 OK`
```json
{
  "message": "Duyệt yêu cầu rút tiền thành công",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440500",
    "collaboratorId": "550e8400-e29b-41d4-a716-446655440100",
    "bankId": "550e8400-e29b-41d4-a716-446655440600",
    "bankName": "Vietcombank",
    "bankAccount": "1234567890",
    "ownerName": "John Doe",
    "amount": 5000.00,
    "taxAmount": 50.00,
    "feeAmount": 10.00,
    "finalAmount": 4940.00,
    "status": "Approved",
    "createdAt": "2025-01-15T10:00:00Z",
    "processedAt": "2025-01-15T11:00:00Z"
  },
  "status": "success"
}
```

---

### POST /api/withdrawals/{id}/reject
Từ chối yêu cầu rút tiền

**Authentication:** Required (Admin role)

**Request Body:**
```json
{
  "reason": "Tài khoản ngân hàng không hợp lệ"
}
```

**Response:** `200 OK`  
(Cấu trúc tương tự POST /api/withdrawals/{id}/approve)

---

## Review API

### POST /api/reviews/contents/{contentId}
Tạo review cho content

**Authentication:** Required

**Request Body:**
```json
{
  "rating": 5,
  "comment": "Sản phẩm rất tuyệt vời, chất lượng tốt!"
}
```

**Lưu ý:** User phải đã mua content để có thể review

**Response:** `200 OK`

---

### GET /api/reviews/contents/{contentId}
Lấy danh sách review của content

**Response:** `200 OK`
```json
{
  "message": "Get reviews successfully",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440700",
      "userId": "550e8400-e29b-41d4-a716-446655440100",
      "contentId": "550e8400-e29b-41d4-a716-446655440000",
      "rating": 5,
      "comment": "Sản phẩm rất tuyệt vời, chất lượng tốt!",
      "createdAt": "2025-01-15T10:00:00Z"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440701",
      "userId": "550e8400-e29b-41d4-a716-446655440101",
      "contentId": "550e8400-e29b-41d4-a716-446655440000",
      "rating": 4,
      "comment": "Tốt nhưng có thể cải thiện thêm",
      "createdAt": "2025-01-14T15:30:00Z"
    }
  ],
  "status": "success"
}
```

---

### PUT /api/reviews/{reviewId}
Cập nhật review

**Authentication:** Required

**Request Body:**
```json
{
  "rating": 4,
  "comment": "Cập nhật: Tốt nhưng có thể cải thiện thêm"
}
```

**Response:** `200 OK`

---

### DELETE /api/reviews/{reviewId}
Xóa review

**Authentication:** Required

**Response:** `204 No Content`

---

## Banner API

### GET /api/banners
Lấy danh sách banner đang hoạt động

**Response:** `200 OK`
```json
{
  "message": "Lấy danh sách banner thành công",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440800",
      "title": "Summer Sale 2025",
      "subtitle": "Save up to 50% on all items",
      "imageUrl": "https://...",
      "button1Text": "Shop Now",
      "button1Link": "/shop",
      "button2Text": "Learn More",
      "button2Link": "/about",
      "isActive": true,
      "displayOrder": 1,
      "createdAt": "2025-01-10T10:00:00Z"
    }
  ],
  "status": "success"
}
```

---

### GET /api/banners/all
Lấy tất cả banner (Bao gồm inactive)

**Authentication:** Required (Admin role)

**Response:** `200 OK`  
(Cấu trúc tương tự GET /api/banners)

---

### POST /api/banners
Tạo banner mới

**Authentication:** Required (Admin role)

**Request Body:** `multipart/form-data`
```
title: "Summer Sale 2025"
subtitle: "Save up to 50% on all items"
button1Text: "Shop Now"
button1Link: "/shop"
button2Text: "Learn More"
button2Link: "/about"
displayOrder: 1
imageFile: File (Banner image)
```

**Response:** `201 Created`  
(Cấu trúc tương tự GET /api/banners)

---

### PUT /api/banners/{id}
Cập nhật banner

**Authentication:** Required (Admin role)

**Request Body:** `multipart/form-data`  
(Cấu trúc tương tự POST /api/banners)

**Response:** `200 OK`

---

### DELETE /api/banners/{id}
Xóa banner

**Authentication:** Required (Admin role)

**Response:** `200 OK`

---

### GET /api/banners/{id}
Lấy chi tiết banner

**Authentication:** Required (Admin role)

**Response:** `200 OK`  
(Cấu trúc tương tự GET /api/banners)

---

## Category API

### GET /api/categories
Lấy danh sách tất cả category

**Response:** `200 OK`
```json
{
  "message": "Lấy danh sách category thành công",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Art"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "name": "Design"
    }
  ],
  "status": "success"
}
```

---

### GET /api/categories/{id}
Lấy chi tiết category

**Response:** `200 OK`  
(Cấu trúc tương tự GET /api/categories item)

---

### POST /api/categories
Tạo category mới

**Authentication:** Required (Admin role)

**Request Body:**
```json
{
  "name": "Photography"
}
```

**Response:** `201 Created`  
(Cấu trúc tương tự GET /api/categories item)

---

### PUT /api/categories/{id}
Cập nhật category

**Authentication:** Required (Admin role)

**Request Body:**
```json
{
  "name": "Photography & Videography"
}
```

**Response:** `200 OK`

---

### DELETE /api/categories/{id}
Xóa category

**Authentication:** Required (Admin role)

**Response:** `200 OK`

---

## Coupon API

### GET /api/coupons
Lấy danh sách tất cả coupon

**Response:** `200 OK`
```json
{
  "message": "Lấy danh sách coupon thành công",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440900",
      "code": "SAVE10",
      "type": "Percentage",
      "value": 10,
      "maxDiscount": 100.00,
      "minOrderAmount": 50.00,
      "usageLimit": 100,
      "usedCount": 45,
      "validFrom": "2025-01-01T00:00:00Z",
      "validTo": "2025-12-31T23:59:59Z",
      "isActive": true,
      "createdAt": "2024-12-01T10:00:00Z"
    }
  ],
  "status": "success"
}
```

**CouponType:** `Percentage` hoặc `FixedAmount`

---

### GET /api/coupons/{code}
Lấy thông tin coupon theo code

**Response:** `200 OK`  
(Cấu trúc tương tự GET /api/coupons item)

---

### POST /api/coupons
Tạo coupon mới

**Authentication:** Required (Admin role)

**Request Body:**
```json
{
  "code": "SAVE10",
  "type": "Percentage",
  "value": 10,
  "maxDiscount": 100.00,
  "minOrderAmount": 50.00,
  "usageLimit": 100,
  "validFrom": "2025-01-01T00:00:00Z",
  "validTo": "2025-12-31T23:59:59Z"
}
```

**Response:** `201 Created`  
(Cấu trúc tương tự GET /api/coupons item)

---

### PUT /api/coupons/{id}
Cập nhật coupon

**Authentication:** Required (Admin role)

**Request Body:**  
(Cấu trúc tương tự POST /api/coupons)

**Response:** `200 OK`

---

### DELETE /api/coupons/{id}
Xóa coupon

**Authentication:** Required (Admin role)

**Response:** `200 OK`

---

## Copyright Report API

### POST /api/copyright-reports
Gửi báo cáo vi phạm bản quyền

**Authentication:** Required

**Request Body:**
```json
{
  "contentId": "550e8400-e29b-41d4-a716-446655440000",
  "reason": "Hình ảnh được sử dụng mà không có phép"
}
```

**Response:** `201 Created`

---

### GET /api/copyright-reports/management
Lấy danh sách tất cả báo cáo

**Authentication:** Required (Admin role)

**Response:** `200 OK`
```json
{
  "message": "Lấy danh sách report thành công",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440950",
      "contentId": "550e8400-e29b-41d4-a716-446655440000",
      "contentTitle": "Beautiful Landscape",
      "reporterId": "550e8400-e29b-41d4-a716-446655440100",
      "reporterEmail": "user@example.com",
      "reason": "Hình ảnh được sử dụng mà không có phép",
      "status": "Pending",
      "createdAt": "2025-01-15T10:00:00Z"
    }
  ],
  "status": "success"
}
```

---

### GET /api/copyright-reports/management/{id}
Lấy chi tiết báo cáo

**Authentication:** Required (Admin role)

**Response:** `200 OK`  
(Cấu trúc tương tự GET /api/copyright-reports/management item)

---

### PATCH /api/copyright-reports/{id}/approve
Phê duyệt báo cáo

**Authentication:** Required (Admin role)

**Response:** `200 OK`

---

### PATCH /api/copyright-reports/{id}/reject
Từ chối báo cáo

**Authentication:** Required (Admin role)

**Response:** `200 OK`

---

## Download API

### GET /api/contents/{contentId}/downloads
Lấy danh sách file có thể download

**Authentication:** Required

**Response:** `200 OK`
```json
{
  "message": "Get downloadable files successfully",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440011",
      "fileName": "landscape_hd.psd",
      "fileUrl": "https://...",
      "fileType": "application/x-psd",
      "size": 5242880,
      "purpose": "Downloadable",
      "displayOrder": 1,
      "createdAt": "2025-01-10T10:00:00Z"
    }
  ],
  "status": "success"
}
```

---

### GET /api/{contentId}/downloads/{fileId}
Download file

**Response:** `200 OK` (File Stream)

---

## Collaborator API

### POST /api/collaborators
Đăng ký trở thành CTV

**Authentication:** Required

**Request Body:**
```json
{
  "reason": "Tôi là họa sĩ chuyên nghiệp"
}
```

**Response:** `202 Accepted`
```json
{
  "message": "Gửi đơn đăng ký collaborator thành công",
  "data": null,
  "status": "success"
}
```

---

### GET /api/collaborators
Lấy danh sách tất cả CTV

**Authentication:** Required (Admin role)

**Response:** `200 OK`
```json
{
  "message": "Lấy danh sách collaborator thành công",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440100",
      "userId": "550e8400-e29b-41d4-a716-446655440002",
      "username": "artist_pro",
      "email": "artist@example.com",
      "status": "Active",
      "commissionRate": 0.10,
      "createdAt": "2025-01-10T10:00:00Z",
      "hasBankAccount": true,
      "banks": [
        {
          "id": "550e8400-e29b-41d4-a716-446655440600",
          "bankName": "Vietcombank",
          "bankAccount": "1234567890",
          "ownerName": "John Doe",
          "isDefault": true
        }
      ]
    }
  ],
  "status": "success"
}
```

---

### PATCH /api/collaborators/{id}/status
Duyệt hoặc từ chối đơn đăng ký CTV

**Authentication:** Required (Admin role)

**Request Body:**
```json
{
  "status": "Approved",
  "comment": "Chào mừng đến với platform!"
}
```

**Status Options:** `Approved`, `Rejected`

**Response:** `200 OK`

---

## Content Statistics API

### GET /api/content-stats
Lấy thống kê content của tôi (CTV)

**Authentication:** Required (Collaborator role)

**Query Parameters:**
```
page=1
pageSize=20
keyword=landscape
```

**Response:** `200 OK`
```json
{
  "message": "Lấy thống kê nội dung của bạn thành công",
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "title": "Beautiful Landscape",
        "thumbnailUrl": "https://...",
        "price": 99.99,
        "status": "Published",
        "createdAt": "2025-01-10T10:00:00Z",
        "views": 150,
        "purchases": 5,
        "downloads": 10,
        "earnings": 499.95
      }
    ],
    "totalCount": 12,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "status": "success"
}
```

---

### GET /api/content-stats/all
Lấy thống kê tất cả content (Admin)

**Authentication:** Required (Admin role)

**Query Parameters:**
```
page=1
pageSize=20
keyword=landscape
collaboratorId=550e8400-e29b-41d4-a716-446655440100
sortBy=Sold (Sold, Views, Downloads, Revenue)
sortDir=Desc (Desc, Asc)
```

**Response:** `200 OK`
```json
{
  "message": "Lấy thống kê tất cả nội dung thành công",
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "title": "Beautiful Landscape",
        "thumbnailUrl": "https://...",
        "price": 99.99,
        "status": "Published",
        "createdAt": "2025-01-10T10:00:00Z",
        "updatedAt": "2025-01-15T14:30:00Z",
        "collaboratorId": "550e8400-e29b-41d4-a716-446655440100",
        "collaboratorUsername": "artist_pro",
        "views": 150,
        "purchases": 5,
        "downloads": 10,
        "totalRevenue": 499.95,
        "conversionRate": 3.33
      }
    ],
    "totalCount": 245,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 13
  },
  "status": "success"
}
```

---

## Error Handling

Tất cả các error response sẽ tuân theo cấu trúc sau:

```json
{
  "message": "Thông báo lỗi tiếng Việt",
  "messageEn": "Error message in English",
  "errors": {
    "fieldName": ["Error 1", "Error 2"]
  },
  "status": "error",
  "statusCode": 400
}
```

**Common Error Codes:**
- `400` - Bad Request (Validation errors)
- `401` - Unauthorized (Missing/Invalid token)
- `403` - Forbidden (Insufficient permissions)
- `404` - Not Found (Resource doesn't exist)
- `409` - Conflict (Resource already exists)
- `500` - Internal Server Error

---

## Authentication

### Headers
```
Authorization: Bearer <access_token>
Content-Type: application/json
```

### Token Expiration
- `accessToken`: 1 giờ
- `refreshToken`: 7 ngày

### Roles
- `user` - Người dùng thông thường
- `collaborator` - CTV (Cộng Tác Viên)
- `admin` - Quản trị viên

---

## Notes

1. **Pagination:** Tất cả endpoints có phân trang sử dụng `page` và `pageSize` query parameters
2. **Timestamps:** Tất cả timestamps sử dụng format ISO 8601 (UTC)
3. **Currency:** Tất cả giá trị tiền tệ sử dụng VND (Vietnamese Dong)
4. **Images:** Tất cả URLs hình ảnh được lưu trữ trên Cloudinary
5. **File Upload:** Sử dụng multipart/form-data cho các endpoints có upload file
6. **Validation:** Tất cả request body validation được thực hiện ở server

---

**Phiên bản:** 1.0.0  
**Cập nhật cuối cùng:** 2025-01-15
