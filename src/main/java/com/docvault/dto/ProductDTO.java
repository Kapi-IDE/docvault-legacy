package com.docvault.dto;

import java.math.BigDecimal;

/**
 * ProductDTO — Jake's attempt to stop exposing JPA entities directly.
 * Created: 2022-07-18 (Jake's last week as intern)
 *
 * Status: HALF DONE. Only ProductDTO was created. No OrderDTO, UserDTO,
 * or CartItemDTO. The controller was partially updated (see ProductController)
 * but Jake's changes conflict with main because Carlos (before leaving) had
 * modified the same controller for the Spanish method rename attempt.
 *
 * Jake's PR #47 ("Add DTOs for REST endpoints") was never reviewed because
 * Priya had already left and Carlos was on his way to Shopify. The branch
 * has been sitting here for 3+ years.
 */
public class ProductDTO {

    private Long id;
    private String name;
    private String description;
    private BigDecimal price;
    private String category;
    private int stockQuantity;
    private String imageUrl;
    private String brand;
    private String sku;
    // Jake: "I'm not including createdAt and updatedAt — clients don't need timestamps"
    // (Some clients do need timestamps. The admin panel broke when he removed them locally.)

    public ProductDTO() {}

    public ProductDTO(Long id, String name, String description, BigDecimal price,
                      String category, int stockQuantity, String imageUrl,
                      String brand, String sku) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.price = price;
        this.category = category;
        this.stockQuantity = stockQuantity;
        this.imageUrl = imageUrl;
        this.brand = brand;
        this.sku = sku;
    }

    // Jake manually wrote all getters/setters instead of using Lombok
    // because "I don't trust annotation processors"
    // (This is 80 lines that Lombok would reduce to 2 annotations.)

    public Long getId() { return id; }
    public void setId(Long id) { this.id = id; }

    public String getName() { return name; }
    public void setName(String name) { this.name = name; }

    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }

    public BigDecimal getPrice() { return price; }
    public void setPrice(BigDecimal price) { this.price = price; }

    public String getCategory() { return category; }
    public void setCategory(String category) { this.category = category; }

    public int getStockQuantity() { return stockQuantity; }
    public void setStockQuantity(int stockQuantity) { this.stockQuantity = stockQuantity; }

    public String getImageUrl() { return imageUrl; }
    public void setImageUrl(String imageUrl) { this.imageUrl = imageUrl; }

    public String getBrand() { return brand; }
    public void setBrand(String brand) { this.brand = brand; }

    public String getSku() { return sku; }
    public void setSku(String sku) { this.sku = sku; }

    /**
     * Convert a Product entity to DTO.
     * Jake: "I'll add a proper MapStruct mapper later"
     * (He did not add a proper MapStruct mapper later.)
     */
    public static ProductDTO fromEntity(com.docvault.model.Product product) {
        return new ProductDTO(
            product.getId(),
            product.getName(),
            product.getDescription(),
            product.getPrice(),
            product.getCategory(),
            product.getStockQuantity(),
            product.getImageUrl(),
            product.getBrand(),
            product.getSku()
        );
    }
}
