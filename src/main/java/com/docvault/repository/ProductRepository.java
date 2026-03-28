package com.docvault.repository;

import com.docvault.model.Product;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface ProductRepository extends JpaRepository<Product, Long> {
    List<Product> findByIsActiveTrue();
    List<Product> findByCategory(String category);
    List<Product> findByCategoryAndIsActiveTrue(String category);
    List<Product> findByBrand(String brand);

    @Query("SELECT p FROM Product p WHERE p.isActive = true AND p.stockQuantity > 0")
    List<Product> findAvailableProducts();

    @Query("SELECT p FROM Product p WHERE p.stockQuantity <= 5 AND p.isActive = true")
    List<Product> findLowStockProducts();
}
