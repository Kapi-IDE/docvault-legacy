package com.docvault.config;

import org.springframework.context.annotation.Configuration;

/**
 * AppConfig — general application configuration.
 *
 * Most config is in application.properties or hardcoded in service classes.
 * This file exists because "Spring best practices say to have a config class"
 * but nothing actually goes here.
 *
 * TODO: Move hardcoded values from OrderService and EmailService here.
 * TODO: Add proper secret management (Vault? AWS SSM? At minimum .env?)
 */
@Configuration
public class AppConfig {

    // Intentionally empty — see TODOs above
    // Carlos planned to move constants here but never got to it

    // These were supposed to be @Value injected from application.properties:
    // private String stripeApiKey;
    // private String smtpPassword;
    // private double taxRate;
    // private double freeShippingThreshold;
}
