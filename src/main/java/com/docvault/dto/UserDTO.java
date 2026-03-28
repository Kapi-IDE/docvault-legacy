package com.docvault.dto;

/**
 * UserDTO — Jake started this on his literal last day.
 * Created: 2022-08-12 (Jake's last day as intern)
 *
 * Status: SKELETON ONLY. Fields defined, no getters/setters, no fromEntity().
 * Jake committed this at 4:47 PM on his last day with the message
 * "wip: will finish monday" — there was no Monday. His internship ended.
 *
 * This is the DTO that was supposed to fix the "password hash exposed in API
 * responses" security vulnerability. If this had been finished, the passwordHash
 * field would be excluded from all REST responses. Instead, every /api/auth/*
 * endpoint still returns the full User entity including the password hash.
 */
public class UserDTO {

    private Long id;
    private String username;
    private String email;
    private String fullName;
    private String role;
    // NOTE: No passwordHash field — that's the whole point of this DTO.
    // But since this DTO is never used, the password hash is still exposed.

    // TODO: Add getters/setters
    // TODO: Add fromEntity() method
    // TODO: Add toEntity() method for registration
    // TODO: Add validation annotations (@NotBlank, @Email, etc.)

    // Jake's sticky note on his monitor (found after he left):
    // "REMEMBER: Update UserController to use UserDTO instead of User entity"
    // "REMEMBER: Add @JsonIgnore to User.passwordHash as a backup"
    // "REMEMBER: Return laptop to IT"
    // (He remembered the laptop. He forgot the other two.)
}
