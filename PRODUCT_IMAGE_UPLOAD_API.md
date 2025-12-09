# Product Image Upload API Documentation

## Overview

The product image upload system allows administrators to upload, manage, and organize multiple images for each product. Images are stored in `wwwroot/images/{productId}/` and referenced in the database.

## Features

- Upload multiple images per product
- Set a main image for each product
- Reorder images
- Delete images
- Automatic file validation (extension, size)
- Unique filename generation to prevent conflicts

## Database Schema

### ProductImage Entity

```csharp
public class ProductImage
{
  public int Id { get; set; }
  public int ProductId { get; set; }
  public required Product Product { get; set; }
  public required string FileName { get; set; }
  public required string RelativePath { get; set; }
  public bool IsMainImage { get; set; }
  public int DisplayOrder { get; set; }
  public DateTime CreatedAt { get; set; }
}
```

## API Endpoints

### Upload Image

```
POST /api/products/{productId}/images
Authorization: Required
```

**Parameters:**

- `productId` (int, path): Product ID
- `file` (IFormFile, form-data): Image file
- `isMainImage` (bool, query): Optional, set as main image. Default: false

**Response (201 Created):**

```json
{
  "imageId": 1,
  "relativePath": "images/5/a1b2c3d4.jpg",
  "isMainImage": false
}
```

**Allowed file types:** `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`
**Max file size:** 5 MB

### Get Product Images

```
GET /api/products/{productId}/images
```

**Response (200 OK):**

```json
[
  {
    "id": 1,
    "relativePath": "images/5/a1b2c3d4.jpg",
    "isMainImage": true,
    "displayOrder": 1
  },
  {
    "id": 2,
    "relativePath": "images/5/e5f6g7h8.jpg",
    "isMainImage": false,
    "displayOrder": 2
  }
]
```

### Get Main Image

```
GET /api/products/{productId}/images/main
```

**Response (200 OK):**

```json
{
  "id": 1,
  "relativePath": "images/5/a1b2c3d4.jpg",
  "isMainImage": true,
  "displayOrder": 1
}
```

### Set Main Image

```
PATCH /api/products/{productId}/images/{imageId}/set-main
Authorization: Admin role required
```

**Response (204 No Content)**

### Delete Image

```
DELETE /api/products/images/{imageId}
Authorization: Admin role required
```

**Response (204 No Content)**

### Reorder Images

```
POST /api/products/{productId}/images/reorder
Authorization: Admin role required
Content-Type: application/json
```

**Request Body:**

```json
[
  { "imageId": 1, "displayOrder": 2 },
  { "imageId": 2, "displayOrder": 1 },
  { "imageId": 3, "displayOrder": 3 }
]
```

**Response (204 No Content)**

## Usage Examples

### Upload Image (cURL)

```bash
curl -X POST "https://api.yourdomain.com/api/products/5/images?isMainImage=true" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@/path/to/image.jpg"
```

### Upload Image (JavaScript/Fetch)

```javascript
const formData = new FormData();
formData.append("file", fileInput.files[0]);

const response = await fetch(`/api/products/5/images?isMainImage=true`, {
  method: "POST",
  headers: {
    Authorization: `Bearer ${token}`,
  },
  body: formData,
});

const result = await response.json();
console.log("Uploaded image:", result.relativePath);
```

### Get Product Images

```javascript
const response = await fetch("/api/products/5/images");
const images = await response.json();

images.forEach((img) => {
  console.log(`Image: ${img.relativePath}, Main: ${img.isMainImage}`);
});
```

### Reorder Images

```javascript
const ordering = [
  { imageId: 2, displayOrder: 1 },
  { imageId: 1, displayOrder: 2 },
  { imageId: 3, displayOrder: 3 },
];

await fetch("/api/products/5/images/reorder", {
  method: "POST",
  headers: {
    Authorization: `Bearer ${token}`,
    "Content-Type": "application/json",
  },
  body: JSON.stringify(ordering),
});
```

## File Storage

Images are stored at: `wwwroot/images/{productId}/{filename}`

Example structure:

```
wwwroot/
├── images/
│   ├── 1/
│   │   ├── a1b2c3d4.jpg
│   │   ├── e5f6g7h8.png
│   │   └── i9j0k1l2.webp
│   ├── 5/
│   │   ├── m3n4o5p6.jpg
│   │   └── q7r8s9t0.jpg
```

## Service Implementation

The `ProductImageService` handles all image operations including:

- File validation
- Directory creation
- Unique filename generation
- Database persistence
- Physical file deletion
- Main image management
- Display order tracking

## Error Handling

All endpoints return appropriate HTTP status codes:

- `201 Created`: Successful upload
- `204 No Content`: Successful update/delete
- `400 Bad Request`: Invalid request (file too large, wrong type, etc.)
- `404 Not Found`: Resource not found
- `401 Unauthorized`: Missing authentication
- `403 Forbidden`: Insufficient permissions
