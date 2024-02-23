window.encryptWithPassword = async function (text, password) {
    const encoder = new TextEncoder();
    const data = encoder.encode(text);

    const passwordKey = await window.crypto.subtle.importKey(
        "raw",
        encoder.encode(password),
        { name: "PBKDF2" },
        false,
        ["deriveBits", "deriveKey"]
    );

    const salt = window.crypto.getRandomValues(new Uint8Array(16));
    const iv = window.crypto.getRandomValues(new Uint8Array(12));

    const key = await window.crypto.subtle.deriveKey(
        {
            name: "PBKDF2",
            salt: salt,
            iterations: 5000,
            hash: "SHA-256"
        },
        passwordKey,
        { name: "AES-GCM", length: 256 },
        true,
        ["encrypt", "decrypt"]
    );

    const cipherTextWithAuthTag = await window.crypto.subtle.encrypt(
        {
            name: "AES-GCM",
            iv: iv
        },
        key,
        data
    );

    const cipherText = cipherTextWithAuthTag.slice(0, cipherTextWithAuthTag.byteLength - 16);
    const authTag = cipherTextWithAuthTag.slice(cipherTextWithAuthTag.byteLength - 16);

    return {
        cipherText: Array.from(new Uint8Array(cipherText)),
        authTag: Array.from(new Uint8Array(authTag)),
        iv: Array.from(iv),
        salt: Array.from(salt)
    };
}

window.decryptWithPassword = async function (encryptedData, password) {
    const { cipherText, authTag, iv, salt } = encryptedData;

    // Combine cipherText and authTag to get the encrypted data as it was before splitting
    const combinedCipherText = new Uint8Array([...cipherText, ...authTag]);

    const encoder = new TextEncoder();
    const decoder = new TextDecoder();

    // Import the password as a key
    const passwordKey = await window.crypto.subtle.importKey(
        "raw",
        encoder.encode(password),
        { name: "PBKDF2" },
        false,
        ["deriveBits", "deriveKey"]
    );

    // Derive the AES-GCM key using the same parameters as in the encrypt function
    const key = await window.crypto.subtle.deriveKey(
        {
            name: "PBKDF2",
            salt: new Uint8Array(salt),
            iterations: 5000,
            hash: "SHA-256"
        },
        passwordKey,
        { name: "AES-GCM", length: 256 },
        true,
        ["encrypt", "decrypt"]
    );

    // Decrypt the data
    const decryptedData = await window.crypto.subtle.decrypt(
        {
            name: "AES-GCM",
            iv: new Uint8Array(iv),
        },
        key,
        combinedCipherText
    );

    // Decode and return the decrypted text
    return decoder.decode(decryptedData);
}

window.sha256HashString = async function (str) {
    // Encode the string into a Uint8Array
    const encoder = new TextEncoder();
    const data = encoder.encode(str);

    // Use the SubtleCrypto API to hash the data with SHA-256
    const hashBuffer = await crypto.subtle.digest('SHA-256', data);

    // Convert the buffer to a byte array
    return Array.from(new Uint8Array(hashBuffer));
}