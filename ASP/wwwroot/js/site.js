class Base64 {
    static #textEncoder = new TextEncoder();
    static #textDecoder = new TextDecoder();

    encode = (str) => btoa(String.fromCharCode(...Base64.#textEncoder.encode(str)));
    decode = (str) => Base64.#textDecoder.decode(Uint8Array.from(atob(str), c => c.charCodeAt(0)));
    encodeUrl = (str) => this.encode(str).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
    decodeUrl = (str) => this.decode(str.replace(/\-/g, '+').replace(/\_/g, '/'));

    jwtEncodeBody = (header, payload) => this.encodeUrl(JSON.stringify(header)) + '.' + this.encodeUrl(JSON.stringify(payload));
    jwtDecodePayload = (jwt) => JSON.parse(this.decodeUrl(jwt.split('.')[1]));
}

document.querySelectorAll('#sign-in-form input').forEach(input => {
    input.addEventListener('input', () => {
        unverified(input);
    });
});

document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id == 'sign-in-form') {
        e.preventDefault();
        const loginInput = form.querySelector('[name="user-login"]');
        if (!loginInput) throw "'loginInput' not found";
        const passwordInput = form.querySelector('[name="user-password"]');
        if (!passwordInput) throw "'passwordInput' not found";
        const btnCancel = document.getElementById("btn-cancel");
        if (!btnCancel) throw "btn-cancel not found";
        btnCancel.onclick = clearForm;
        const alertContainer = document.getElementById("auth-error");

        let hasError = false;

        if (!loginInput.value.trim()) {
            showError(loginInput, "Логін не може бути порожнім");
            hasError = true;
        } else markValid(loginInput);

        if (!passwordInput.value.trim()) {
            showError(passwordInput, "Пароль не може бути порожнім");
            hasError = true;
        } else markValid(passwordInput);

        if (hasError) return;
        const credentials = new Base64().encode(
            `${loginInput.value}:${passwordInput.value}`);
        fetch('/User/SignIn', {
            method: 'GET',
            headers: {
                'Authorization': `Basic ${credentials}`
            }
        }).then(r => r.json())
            .then(j => {
                console.log(j);
                if (j.status == 200) {
                    window.location.reload();
                }
                else {
                    showAuthError(j.data || "Невірний логін або пароль");
                    console.log(j.data);
                }
            }
            );
        console.log(loginInput.value + " " + passwordInput.value);
    }
});

function unverified(input) {
    input.classList.remove("is-invalid");
    input.classList.remove("is-valid");
    const feedback = input.parentElement.querySelector(".invalid-feedback");
    feedback.style.display = "none"
}

function showError(input, message){
    input.classList.add("is-invalid");
    input.classList.remove("is-valid");

    const feedback = input.parentElement.querySelector(".invalid-feedback");
    if (feedback) {
        feedback.textContent = message;
        feedback.style.display = "block";
    }
}

function markValid(input){
    input.classList.remove("is-invalid");
    input.classList.add("is-valid");
    const feedback = input.parentElement.querySelector(".invalid-feedback");
    if (feedback) feedback.style.display = "none";
}

function clearForm() {
    const form = document.getElementById("sign-in-form");
    if (!form) throw "'sign-in-form' not found";

    const loginInput = form.querySelector('[name="user-login"]');
    if (!loginInput) throw "'loginInput' not found";
    const passwordInput = form.querySelector('[name="user-password"]');
    if (!passwordInput) throw "'passwordInput' not found";

    loginInput.value = "";
    passwordInput.value = "";

    unverified(loginInput);
    unverified(passwordInput);

    const feedbackLogin = loginInput.parentElement.querySelector(".invalid-feedback");
    if (feedbackLogin) feedbackLogin.textContent = "";

    const feedbackPassword = passwordInput.parentElement.querySelector(".invalid-feedback");
    if (feedbackPassword) feedbackPassword.textContent = "";
}

function showAuthError(message) {
    const alert = document.getElementById("auth-error");
    alert.textContent = message;
    alert.classList.remove("d-none", "fade");
    alert.classList.add("show");
}
