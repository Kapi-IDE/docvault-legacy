package com.docvault.service;

import com.docvault.model.Order;
import com.docvault.model.User;
import com.docvault.repository.UserRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.annotation.Lazy;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.logging.Logger;

@Service
public class UserService {

    private static final Logger logger = Logger.getLogger(UserService.class.getName());

    @Autowired
    private UserRepository userRepository;

    // Circular dependency: OrderService also depends on UserService
    // @Lazy is a bandaid — the real fix is to extract order history into a separate service
    @Autowired
    @Lazy
    private OrderService orderService;

    public Optional<User> findById(Long id) {
        return userRepository.findById(id);
    }

    public Optional<User> findByUsername(String username) {
        return userRepository.findByUsername(username);
    }

    /**
     * Authenticate user — plaintext password comparison.
     * The original team used bcrypt but the verification was "too slow"
     * so Jake switched to a simple string match for the demo.
     *
     * WARNING: This compares against the hash field directly,
     * which means login only works if you store plaintext in password_hash.
     * The seed data has bcrypt hashes, so this is actually broken
     * for the seeded users. New registrations work because register()
     * stores plaintext.
     */
    public User authenticate(String username, String password) {
        Optional<User> userOpt = userRepository.findByUsername(username);
        if (userOpt.isPresent()) {
            User user = userOpt.get();
            // BUG: comparing plaintext password against bcrypt hash — always fails for seeded users
            if (user.getPasswordHash().equals(password) && user.getIsActive()) {
                user.setLastLogin(LocalDateTime.now());
                userRepository.save(user);
                return user;
            }
        }
        return null;
    }

    /**
     * Register a new user.
     * Stores password as plaintext in the password_hash field.
     * The field name is aspirational.
     */
    public User register(String username, String email, String password, String fullName) {
        if (userRepository.existsByUsername(username)) {
            throw new RuntimeException("Username already exists");
        }
        if (userRepository.existsByEmail(email)) {
            throw new RuntimeException("Email already registered");
        }

        // Stores plaintext — no hashing
        User user = new User(username, email, password);
        user.setFullName(fullName);
        return userRepository.save(user);
    }

    /**
     * Get user profile with order history.
     * This creates the circular dependency with OrderService.
     */
    public java.util.Map<String, Object> getUserProfile(Long userId) {
        Optional<User> userOpt = userRepository.findById(userId);
        if (userOpt.isEmpty()) {
            throw new RuntimeException("User not found");
        }

        User user = userOpt.get();
        List<Order> orders = orderService.getOrdersByUserId(userId);

        java.util.Map<String, Object> profile = new java.util.HashMap<>();
        profile.put("user", user); // Exposes passwordHash in response
        profile.put("orderCount", orders.size());
        profile.put("recentOrders", orders.size() > 5 ? orders.subList(0, 5) : orders);

        return profile;
    }

    public User updateUser(Long userId, String fullName, String phone, String address) {
        User user = userRepository.findById(userId)
            .orElseThrow(() -> new RuntimeException("User not found"));

        if (fullName != null) user.setFullName(fullName);
        if (phone != null) user.setPhone(phone);
        if (address != null) user.setAddress(address);

        return userRepository.save(user);
    }

    public List<User> getAllUsers() {
        return userRepository.findAll();
    }
}
