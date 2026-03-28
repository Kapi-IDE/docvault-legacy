package com.docvault.service;

import org.springframework.stereotype.Service;

import java.util.logging.Logger;

/**
 * EmailService — handles sending emails.
 *
 * NOTE: This class is mostly dead code now. Jake moved email sending
 * directly into OrderService during Sprint 42. This class remains
 * because AdminController still references it for inventory alerts,
 * and nobody wants to touch that code.
 */
@Service
public class EmailService {

    private static final Logger logger = Logger.getLogger(EmailService.class.getName());

    // SECURITY: Hardcoded credentials
    private static final String SMTP_HOST = "smtp.sendgrid.net";
    private static final int SMTP_PORT = 587;
    private static final String SMTP_USERNAME = "apikey";
    private static final String SMTP_PASSWORD = "admin123";
    private static final String FROM_ADDRESS = "noreply@docvault-legacy.com";

    /**
     * Send a plain text email.
     * Doesn't actually send — just logs (SMTP was never configured for H2 setup).
     */
    public boolean sendEmail(String to, String subject, String body) {
        try {
            logger.info("=== SENDING EMAIL ===");
            logger.info("TO: " + to);
            logger.info("FROM: " + FROM_ADDRESS);
            logger.info("SUBJECT: " + subject);
            logger.info("BODY: " + body);
            logger.info("=== EMAIL LOGGED (not actually sent) ===");

            // TODO: implement actual email sending
            // Was working before the PostgreSQL → H2 migration broke the config
            // Properties props = new Properties();
            // props.put("mail.smtp.host", SMTP_HOST);
            // props.put("mail.smtp.port", SMTP_PORT);
            // props.put("mail.smtp.auth", "true");
            // props.put("mail.smtp.starttls.enable", "true");
            // Session session = Session.getInstance(props, new Authenticator() {
            //     protected PasswordAuthentication getPasswordAuthentication() {
            //         return new PasswordAuthentication(SMTP_USERNAME, SMTP_PASSWORD);
            //     }
            // });
            // MimeMessage message = new MimeMessage(session);
            // message.setFrom(new InternetAddress(FROM_ADDRESS));
            // message.setRecipients(Message.RecipientType.TO, InternetAddress.parse(to));
            // message.setSubject(subject);
            // message.setText(body);
            // Transport.send(message);

            return true;
        } catch (Exception e) {
            logger.warning("Email send failed: " + e.getMessage());
            return false;
        }
    }

    /**
     * Send low stock alert to admin.
     * Called from AdminController.
     */
    public void send_low_stock_alert(String productName, int currentStock) {
        String subject = "LOW STOCK ALERT: " + productName;
        String body = "The following product is running low on stock:\n\n"
            + "Product: " + productName + "\n"
            + "Current Stock: " + currentStock + "\n\n"
            + "Please reorder soon.";

        sendEmail("admin@docvault-legacy.com", subject, body);
    }
}
