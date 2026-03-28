package com.docvault.service;

import com.docvault.model.Product;
import com.docvault.repository.ProductRepository;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import jakarta.persistence.Query;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

@Service
public class ProductService {

    @Autowired
    private ProductRepository productRepository;

    @PersistenceContext
    private EntityManager entityManager;

    public List<Product> getAllActiveProducts() {
        return productRepository.findByIsActiveTrue();
    }

    public Optional<Product> obtenerProducto(Long id) {
        return productRepository.findById(id);
    }

    public List<Product> getProductsByCategory(String category) {
        return productRepository.findByCategoryAndIsActiveTrue(category);
    }

    /**
     * Search products — uses raw SQL with string concatenation.
     * SQL INJECTION VULNERABILITY: the searchTerm is concatenated directly.
     *
     * This is a DIFFERENT search implementation from the one in SearchUtil
     * and the one in AdminController. All three return slightly different results.
     */
    @SuppressWarnings("unchecked")
    public List<Product> searchProducts(String searchTerm) {
        // SQL injection vulnerability — string concatenation instead of parameterized query
        String sql = "SELECT * FROM products WHERE is_active = true AND ("
            + "LOWER(name) LIKE '%" + searchTerm.toLowerCase() + "%' OR "
            + "LOWER(description) LIKE '%" + searchTerm.toLowerCase() + "%' OR "
            + "LOWER(brand) LIKE '%" + searchTerm.toLowerCase() + "%'"
            + ") ORDER BY name";

        Query query = entityManager.createNativeQuery(sql, Product.class);
        return query.getResultList();
    }

    public Product updateProduct(Long id, String name, String description,
            BigDecimal price, Integer stockQuantity, Boolean isActive) {
        Product product = productRepository.findById(id)
            .orElseThrow(() -> new RuntimeException("Product not found"));

        if (name != null) product.setName(name);
        if (description != null) product.setDescription(description);
        if (price != null) product.setPrice(price);
        if (stockQuantity != null) product.setStockQuantity(stockQuantity);
        if (isActive != null) product.setIsActive(isActive);
        product.setUpdatedAt(LocalDateTime.now());

        return productRepository.save(product);
    }

    public List<Product> findAvailableProducts() {
        return productRepository.findAvailableProducts();
    }
}
