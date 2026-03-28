package com.docvault.controller;

import com.docvault.model.Product;
import com.docvault.repository.ProductRepository;
import com.docvault.service.ProductService;
import com.docvault.util.SearchUtil;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.math.BigDecimal;
import java.util.List;

@RestController
@RequestMapping("/api/products")
public class ProductController {

    @Autowired
    private ProductService productService;

    // Inconsistent: some endpoints go through service, some call repository directly
    @Autowired
    private ProductRepository productRepository;

    @GetMapping
    public ResponseEntity<List<Product>> getAllProducts() {
        return ResponseEntity.ok(productService.getAllActiveProducts());
    }

    @GetMapping("/{id}")
    public ResponseEntity<Product> getProduct(@PathVariable Long id) {
        // Uses Spanish method name from ProductService
        return productService.obtenerProducto(id)
            .map(ResponseEntity::ok)
            .orElse(ResponseEntity.notFound().build());
    }

    /**
     * Search products — uses ProductService which has SQL injection vuln.
     * There's also SearchUtil.filterProducts() which does the same thing differently.
     */
    @GetMapping("/search")
    public ResponseEntity<List<Product>> searchProducts(@RequestParam String q) {
        if (q == null || q.trim().isEmpty()) {
            return ResponseEntity.ok(productService.getAllActiveProducts());
        }
        return ResponseEntity.ok(productService.searchProducts(q));
    }

    @GetMapping("/category/{category}")
    public ResponseEntity<List<Product>> getByCategory(@PathVariable String category) {
        return ResponseEntity.ok(productService.getProductsByCategory(category));
    }

    /**
     * Advanced search using SearchUtil (the in-memory version).
     * Returns different results than /search for the same query
     * because SearchUtil doesn't check the brand field.
     */
    @GetMapping("/filter")
    public ResponseEntity<List<Product>> filterProducts(
            @RequestParam(required = false) String q,
            @RequestParam(required = false) String category,
            @RequestParam(required = false) BigDecimal minPrice,
            @RequestParam(required = false) BigDecimal maxPrice,
            @RequestParam(required = false) String sort) {

        // Bypasses service layer — goes directly to repository
        List<Product> allProducts = productRepository.findByIsActiveTrue();
        List<Product> filtered = SearchUtil.filterProducts(allProducts, q, category, minPrice, maxPrice, sort);
        return ResponseEntity.ok(filtered);
    }

    @GetMapping("/available")
    public ResponseEntity<List<Product>> getAvailableProducts() {
        return ResponseEntity.ok(productService.findAvailableProducts());
    }
}
