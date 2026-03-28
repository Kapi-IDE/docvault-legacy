package com.docvault.controller;

import com.docvault.model.Order;
import com.docvault.service.OrderService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api")
public class OrderController {

    @Autowired
    private OrderService orderService;

    /**
     * Place an order (checkout).
     * Accepts raw Map because there are no DTOs.
     * No input validation whatsoever.
     */
    @PostMapping("/checkout")
    public ResponseEntity<Map<String, Object>> checkout(@RequestBody Map<String, Object> request) {
        Long userId = Long.valueOf(request.get("userId").toString());
        String paymentMethod = (String) request.getOrDefault("paymentMethod", "CREDIT_CARD");
        String shippingAddress = (String) request.get("shippingAddress");
        String cardNumber = (String) request.get("cardNumber");
        String cardExpiry = (String) request.get("cardExpiry");
        String cardCvv = (String) request.get("cardCvv");
        String notes = (String) request.get("notes");

        Map<String, Object> result = orderService.processCheckout(
            userId, paymentMethod, shippingAddress, cardNumber, cardExpiry, cardCvv, notes
        );

        if (Boolean.TRUE.equals(result.get("success"))) {
            return ResponseEntity.ok(result);
        } else {
            return ResponseEntity.badRequest().body(result);
        }
    }

    @GetMapping("/orders")
    public ResponseEntity<List<Order>> getOrders(@RequestParam Long userId) {
        // Returns Order entities directly — includes User entity with passwordHash
        return ResponseEntity.ok(orderService.getOrdersByUserId(userId));
    }

    @GetMapping("/orders/{orderId}")
    public ResponseEntity<Order> getOrder(@PathVariable Long orderId) {
        return orderService.getOrderById(orderId)
            .map(ResponseEntity::ok)
            .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping("/orders/search")
    public ResponseEntity<List<Order>> searchOrders(
            @RequestParam(required = false) String q,
            @RequestParam(required = false) String status,
            @RequestParam(required = false) Long userId) {
        // Uses the SQL injection vulnerable search method
        return ResponseEntity.ok(orderService.searchOrders(q, status, userId));
    }
}
