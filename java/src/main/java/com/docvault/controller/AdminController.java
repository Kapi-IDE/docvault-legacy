package com.docvault.controller;

import com.docvault.model.Product;
import com.docvault.repository.ProductRepository;
import com.docvault.service.EmailService;
import com.docvault.service.OrderService;
import com.docvault.service.ProductService;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.math.BigDecimal;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * AdminController — admin-only endpoints.
 *
 * NOTE: There is NO authentication check. Any user can call these endpoints.
 * "We'll add auth later" — Carlos, 2019
 */
@RestController
@RequestMapping("/api/admin")
public class AdminController {

    @Autowired
    private ProductService productService;

    @Autowired
    private OrderService orderService;

    @Autowired
    private EmailService emailService;

    // Yet another direct repository access from a controller
    @Autowired
    private ProductRepository productRepository;

    @PersistenceContext
    private EntityManager entityManager;

    @GetMapping("/products")
    public ResponseEntity<List<Product>> getAllProducts() {
        // Returns ALL products (including inactive), unlike ProductController
        return ResponseEntity.ok(productRepository.findAll());
    }

    @PutMapping("/products/{id}")
    public ResponseEntity<?> updateProduct(@PathVariable Long id,
                                           @RequestBody Map<String, Object> updates) {
        try {
            String name = (String) updates.get("name");
            String description = (String) updates.get("description");
            BigDecimal price = updates.containsKey("price")
                ? new BigDecimal(updates.get("price").toString()) : null;
            Integer stock = updates.containsKey("stockQuantity")
                ? Integer.parseInt(updates.get("stockQuantity").toString()) : null;
            Boolean isActive = updates.containsKey("isActive")
                ? (Boolean) updates.get("isActive") : null;

            Product updated = productService.updateProduct(id, name, description, price, stock, isActive);

            // Check low stock and send alert (uses EmailService, not OrderService's inline version)
            if (updated.getStockQuantity() != null && updated.getStockQuantity() <= 5) {
                emailService.send_low_stock_alert(updated.getName(), updated.getStockQuantity());
            }

            return ResponseEntity.ok(updated);
        } catch (RuntimeException e) {
            return ResponseEntity.badRequest().body(Map.of("error", e.getMessage()));
        }
    }

    /**
     * Inventory report — yet another place that queries products.
     * Uses JPQL instead of repository methods or raw SQL.
     * Fourth different way to query products in this codebase.
     */
    @GetMapping("/inventory")
    public ResponseEntity<?> getInventoryReport() {
        @SuppressWarnings("unchecked")
        List<Product> allProducts = entityManager.createQuery(
            "SELECT p FROM Product p ORDER BY p.stockQuantity ASC"
        ).getResultList();

        int totalProducts = allProducts.size();
        int outOfStock = 0;
        int lowStock = 0;
        int healthyStock = 0;
        BigDecimal totalValue = BigDecimal.ZERO;

        for (Product p : allProducts) {
            if (p.getStockQuantity() == null || p.getStockQuantity() == 0) {
                outOfStock++;
            } else if (p.getStockQuantity() <= 5) {
                lowStock++;
            } else {
                healthyStock++;
            }

            if (p.getStockQuantity() != null && p.getPrice() != null) {
                totalValue = totalValue.add(
                    p.getPrice().multiply(BigDecimal.valueOf(p.getStockQuantity()))
                );
            }
        }

        Map<String, Object> report = new HashMap<>();
        report.put("totalProducts", totalProducts);
        report.put("outOfStock", outOfStock);
        report.put("lowStock", lowStock);
        report.put("healthyStock", healthyStock);
        report.put("totalInventoryValue", totalValue);
        report.put("products", allProducts);

        return ResponseEntity.ok(report);
    }

    /**
     * Admin search — yet ANOTHER search implementation.
     * Uses JPQL, searches name and category only (not description or brand).
     * Different from ProductService, SearchUtil, and OrderService searches.
     */
    @GetMapping("/products/search")
    public ResponseEntity<List<Product>> adminSearchProducts(@RequestParam String q) {
        @SuppressWarnings("unchecked")
        List<Product> results = entityManager.createQuery(
            "SELECT p FROM Product p WHERE LOWER(p.name) LIKE :term OR LOWER(p.category) LIKE :term ORDER BY p.name"
        ).setParameter("term", "%" + q.toLowerCase() + "%")
         .getResultList();

        return ResponseEntity.ok(results);
    }

    @GetMapping("/orders/stats")
    public ResponseEntity<Map<String, Object>> getOrderStats() {
        return ResponseEntity.ok(orderService.getOrderStats());
    }

    @PutMapping("/orders/{orderId}/status")
    public ResponseEntity<?> updateOrderStatus(@PathVariable Long orderId,
                                               @RequestBody Map<String, String> request) {
        try {
            String newStatus = request.get("status");
            return ResponseEntity.ok(orderService.updateOrderStatus(orderId, newStatus));
        } catch (RuntimeException e) {
            return ResponseEntity.badRequest().body(Map.of("error", e.getMessage()));
        }
    }
}
