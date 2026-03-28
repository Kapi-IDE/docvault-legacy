package com.docvault.controller;

import com.docvault.dto.ProductDTO;
import com.docvault.model.Product;
import com.docvault.repository.ProductRepository;
import com.docvault.service.ProductService;
import com.docvault.util.SearchUtil;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.math.BigDecimal;
import java.util.List;
import java.util.stream.Collectors;

/**
 * ProductController — partially migrated to DTOs by Jake.
 *
 * NOTE: Some endpoints return ProductDTO, others still return Product entity.
 * Jake ran out of time on his last week. The getAllProducts and getProduct
 * endpoints were converted, but search/filter/category were not.
 * This means the API is INCONSISTENT — some responses have timestamps
 * and internal fields, others don't. Clients must handle both shapes.
 *
 * DO NOT merge this branch without finishing the DTO migration for ALL endpoints.
 * (This note was written by Jake. Nobody read it.)
 */
@RestController
@RequestMapping("/api/products")
public class ProductController {

    @Autowired
    private ProductService productService;

    // Inconsistent: some endpoints go through service, some call repository directly
    @Autowired
    private ProductRepository productRepository;

    // CONVERTED TO DTO by Jake
    @GetMapping
    public ResponseEntity<List<ProductDTO>> getAllProducts() {
        List<ProductDTO> dtos = productService.getAllActiveProducts()
            .stream()
            .map(ProductDTO::fromEntity)
            .collect(Collectors.toList());
        return ResponseEntity.ok(dtos);
    }

    // CONVERTED TO DTO by Jake
    @GetMapping("/{id}")
    public ResponseEntity<ProductDTO> getProduct(@PathVariable Long id) {
        return productService.obtenerProducto(id)
            .map(ProductDTO::fromEntity)
            .map(ResponseEntity::ok)
            .orElse(ResponseEntity.notFound().build());
    }

    // NOT CONVERTED — Jake ran out of time
    // Still returns raw Product entity with all JPA fields
    @GetMapping("/search")
    public ResponseEntity<List<Product>> searchProducts(@RequestParam String q) {
        if (q == null || q.trim().isEmpty()) {
            return ResponseEntity.ok(productService.getAllActiveProducts());
        }
        return ResponseEntity.ok(productService.searchProducts(q));
    }

    // NOT CONVERTED — Jake ran out of time
    @GetMapping("/category/{category}")
    public ResponseEntity<List<Product>> getByCategory(@PathVariable String category) {
        return ResponseEntity.ok(productService.getProductsByCategory(category));
    }

    // NOT CONVERTED — Jake ran out of time
    @GetMapping("/filter")
    public ResponseEntity<List<Product>> filterProducts(
            @RequestParam(required = false) String q,
            @RequestParam(required = false) String category,
            @RequestParam(required = false) BigDecimal minPrice,
            @RequestParam(required = false) BigDecimal maxPrice,
            @RequestParam(required = false) String sort) {

        List<Product> allProducts = productRepository.findByIsActiveTrue();
        List<Product> filtered = SearchUtil.filterProducts(allProducts, q, category, minPrice, maxPrice, sort);
        return ResponseEntity.ok(filtered);
    }

    // NOT CONVERTED — Jake ran out of time
    @GetMapping("/available")
    public ResponseEntity<List<Product>> getAvailableProducts() {
        return ResponseEntity.ok(productService.findAvailableProducts());
    }
}
