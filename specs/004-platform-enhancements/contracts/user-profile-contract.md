# Contract: User Profile & Password Change Protocol

**Feature Branch**: `004-platform-enhancements`  
**Date**: 2026-11-21  

This contract defines the API endpoints and request/response models for user profile updates and self-service password changes.

---

## 1. Endpoints

### 1.1 `GET /api/user/profile/{userId}`
Retrieves user profile details.

#### Response Body:
```json
{
  "id": 1,
  "userName": "admin",
  "displayName": "Admin Master",
  "email": "admin@nangworld.com",
  "avatarUrl": "/images/avatars/pastel-1.png",
  "bio": "Movie Enthusiast & Admin",
  "roles": ["Admin", "SuperAdmin"],
  "createdDate": "2026-08-10T12:00:00Z"
}
```

---

### 1.2 `POST /api/user/profile/avatar`
Uploads a local avatar image (JPG/PNG, up to 5MB) using multipart/form-data.

#### Request:
- Form Data: `avatarFile` (`IFormFile`)
- Header: `Content-Type: multipart/form-data`
- Route param or query: `userId`

#### Response Body:
```json
{
  "isSuccess": true,
  "message": "Avatar uploaded successfully.",
  "avatarUrl": "/uploads/images/a1b2c3d4.jpg"
}
```

---

### 1.3 `PUT /api/user/profile`
Updates user profile information (display name, bio).

#### Request Body:
```json
{
  "userId": 1,
  "displayName": "Nang Star",
  "bio": "Watching Korean dramas day and night."
}
```

#### Response Body:
```json
{
  "isSuccess": true,
  "message": "Profile updated successfully.",
  "user": {
    "id": 1,
    "displayName": "Nang Star",
    "avatarUrl": "/uploads/images/a1b2c3d4.jpg"
  }
}
```

---

### 1.4 `POST /api/user/change-password`
Changes user password with verification.

#### Request Body:
```json
{
  "userId": 1,
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword456!",
  "confirmNewPassword": "NewPassword456!"
}
```

#### Response Body (Success):
```json
{
  "isSuccess": true,
  "message": "Password changed successfully."
}
```

#### Response Body (Failure - Incorrect Current Password):
```json
{
  "isSuccess": false,
  "message": "Current password is incorrect."
}
```

#### Response Body (Failure - Validation):
```json
{
  "isSuccess": false,
  "message": "New password must be at least 6 characters and cannot match current password."
}
```
