package com.docvault.service;

import com.docvault.model.Order;
import com.docvault.model.User;
import com.docvault.repository.UserRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.HashMap;
import java.util.Map;
import java.util.logging.Logger;

/**
 * RewardsEngine — Carlos's loyalty points prototype.
 * Started: 2021-09-15
 * Status: NEVER FINISHED. Carlos left before connecting it to anything.
 *
 * Points system:
 *   - 1 point per $1 spent
 *   - 2x points on Tuesdays ("Treat Tuesday")
 *   - 500 points = $5 discount
 *   - Points expire after 90 days
 *
 * NOTE: This class is NOT wired into any controller or service.
 * The @Service annotation means Spring creates it, but nothing injects it.
 * It just sits in memory doing nothing. Classic Carlos.
 *
 * DO NOT DELETE — Carlos said "I'll finish it when I get back from PTO"
 * (He never came back from PTO. He went to Shopify.)
 */
@Service
public class RewardsEngine {

    private static final Logger logger = Logger.getLogger(RewardsEngine.class.getName());

    private static final BigDecimal POINTS_PER_DOLLAR = BigDecimal.ONE;
    private static final int TUESDAY_MULTIPLIER = 2;
    private static final int REDEMPTION_THRESHOLD = 500;
    private static final BigDecimal REDEMPTION_VALUE = new BigDecimal("5.00");
    private static final int EXPIRY_DAYS = 90;

    // In-memory points ledger (was supposed to be a database table)
    // Carlos: "I'll add the schema migration next sprint"
    private final Map<Long, Integer> pointsLedger = new HashMap<>();
    private final Map<Long, LocalDateTime> lastEarned = new HashMap<>();

    @Autowired
    private UserRepository userRepository;

    /**
     * Award points for a completed order.
     * Never called from anywhere.
     */
    public void awardPoints(User user, Order order) {
        int basePoints = order.getTotalAmount().intValue();

        // Treat Tuesday — 2x points
        int dayOfWeek = LocalDateTime.now().getDayOfWeek().getValue();
        int multiplier = (dayOfWeek == 2) ? TUESDAY_MULTIPLIER : 1;

        int earned = basePoints * multiplier;

        int current = pointsLedger.getOrDefault(user.getId(), 0);
        pointsLedger.put(user.getId(), current + earned);
        lastEarned.put(user.getId(), LocalDateTime.now());

        logger.info("Awarded " + earned + " points to user " + user.getId()
            + " (total: " + (current + earned) + ")");
    }

    /**
     * Check if user can redeem points.
     * Never called from anywhere.
     */
    public boolean canRedeem(Long userId) {
        int points = pointsLedger.getOrDefault(userId, 0);
        LocalDateTime lastActivity = lastEarned.get(userId);

        // Check expiry
        if (lastActivity != null && lastActivity.plusDays(EXPIRY_DAYS).isBefore(LocalDateTime.now())) {
            pointsLedger.put(userId, 0);
            logger.info("Points expired for user " + userId);
            return false;
        }

        return points >= REDEMPTION_THRESHOLD;
    }

    /**
     * Redeem points for a discount.
     * Never called from anywhere.
     *
     * TODO: This should return a discount code, not a BigDecimal.
     * TODO: Need to integrate with Stripe coupon API.
     * TODO: Need to deduct points after redemption.
     * (None of these TODOs will ever be done.)
     */
    public BigDecimal redeem(Long userId) {
        if (!canRedeem(userId)) {
            return BigDecimal.ZERO;
        }
        // BUG: Doesn't actually deduct points. Carlos's note: "will fix"
        return REDEMPTION_VALUE;
    }

    /**
     * Get points balance.
     * Never called from anywhere.
     */
    public int getBalance(Long userId) {
        return pointsLedger.getOrDefault(userId, 0);
    }

    /**
     * Bulk expire points for all users.
     * Was supposed to be a scheduled job. Cron annotation was never added.
     *
     * Carlos: "I'll set up the @Scheduled config this week"
     * Narrator: He did not set up the @Scheduled config that week.
     */
    public void expireStalePoints() {
        LocalDateTime cutoff = LocalDateTime.now().minusDays(EXPIRY_DAYS);
        int expired = 0;

        for (Map.Entry<Long, LocalDateTime> entry : lastEarned.entrySet()) {
            if (entry.getValue().isBefore(cutoff)) {
                pointsLedger.put(entry.getKey(), 0);
                expired++;
            }
        }

        logger.info("Expired points for " + expired + " users");
    }
}
