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

function showError(input, message) {
    input.classList.add("is-invalid");
    input.classList.remove("is-valid");

    const feedback = input.parentElement.querySelector(".invalid-feedback");
    if (feedback) {
        feedback.textContent = message;
        feedback.style.display = "block";
    }
}

function markValid(input) {
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


document.addEventListener('DOMContentLoaded', () => {
    for (let btn of document.querySelectorAll('[data-nav]')) {
        btn.onclick = navigate;
    }
});

function navigate(e) {
    const targetBtn = e.target.closest('[data-nav]');
    const route = targetBtn.getAttribute('data-nav');
    if (!route) throw "route not found";
    for (let btn of document.querySelectorAll('[data-nav]')) {
        btn.classList.remove("active");
    }
    targetBtn.classList.add("active");
    showPage(route)
}

function showPage(page) {
    window.activePage = page;
    const spaContainer = document.getElementById("spa-container");
    if (!spaContainer) throw "spa-container not found";

    switch (page) {
        case 'home': spaContainer.innerHTML = `<b>Home</b>`; break;
        case 'privacy': spaContainer.innerHTML = `<b>Privacy</b>`; break;
        case 'auth': spaContainer.innerHTML = !!window.accessToken ? profileHtml : authHtml; break;
        default: spaContainer.innerHTML = `<b>404</b>`;
    }
}

const authHtml = `<div>
    <div class="input-group mb-3">
        <span class="input-group-text" id="user-login-addon"><i class="bi bi-key"></i></span>
        <input name="user-login" type="text" class="form-control" placeholder="Login"
                aria-label="Login" aria-describedby="user-login-addon">
        <div class="invalid-feedback"></div>
    </div>
    <div class="input-group mb-3">
        <span class="input-group-text" id="user-password-addon"><i class="bi bi-unlock"></i></span>
        <input name="user-password" type="password" class="form-control" placeholder="Password"
                aria-label="Password" aria-describedby="user-password-addon">
        <div class="invalid-feedback"></div>
    </div>
    <button type="submit" class="btn btn-primary" onclick="authClick()">Вхід</button>
</div>`;

const profileHtml = `<div>
<h3>Вітаємо у кабінеті</h3>
<button type="button" class="btn btn-success" onclick="emailClick()">Лист</button>
<button type="button" class="btn btn-warning" onclick="exitClick()">Вихід</button>
</div>`

function authClick() {
    const login = document.querySelector('input[name="user-login"]').value;
    const password = document.querySelector('input[name="user-password"]').value;
    console.log(login, password);

    const credentials = new Base64().encode(`${login}:${password}`);
    fetch('/User/LogIn', {
        method: 'GET',
        headers: {
            'Authorization': `Basic ${credentials}`
        }
    }).then(r => r.json())
        .then(j => {
            if (j.status == 200) {
                window.accessToken = j.data;
                //console.log(window.accessToken);
                window.accessToken.exp = Number(j.data.exp);
                //tokenExpCheck();
                showPage(window.activePage);
            }
            else {
                alert("Rejected");
            }
        });
}

let tokenInterval;

function exitClick() {
    window.accessToken = null;
    clearInterval(tokenInterval);
    showPage(window.activePage);
}

function tokenExpCheck() {
    clearInterval(tokenInterval);

    tokenInterval = setInterval(() => {
        const exp = window.accessToken?.exp;
        const now = Date.now();
        //console.log("EXP:", exp, "NOW:", now, "LEFT:", (exp - now) / 1000);
        if (exp && now >= exp) {
            alert("Сесія завершена. Повторіть вхід.");
            exitClick();
        }
    }, 1000); 
}

function emailClick() {
    fetch("/User/Email", {
        method: "POST",
        headers: {
            "Authorization": "Bearer " + window.accessToken
        }
    }).then(r => r.json())
    .then(console.log);
}

document.addEventListener('DOMContentLoaded', () => {
    const editProfileBtn = document.getElementById("edit-profile-btn");
    if (editProfileBtn) {
        editProfileBtn.onclick = editProfileBtnClick;
    }

    const deleteProfileBtn = document.getElementById("delete-profile-btn");
    if (deleteProfileBtn) {
        deleteProfileBtn.onclick = deleteProfileBtnClick;
    }
});
function editProfileBtnClick() {
    let changes = [];
    for (let elem of document.querySelectorAll('[data-editable]')) {
        if (elem.getAttribute('contenteditable')) {
            elem.removeAttribute('contenteditable');
            console.log(elem.originalData, elem.innerText);
            if (elem.originalData != elem.innerText) {
                changes.push({
                    field: elem.getAttribute('data-editable'),
                    value: elem.innerText
                });
            }
        }
        else {
            elem.setAttribute('contenteditable', true);
            elem.originalData = elem.innerText;
        }
        if (changes.length > 0) {
            const msg = changes
                .map(c => `${c.field}=${c.value}`)
                .join(', ');
            if (confirm(`Update data: ${msg}`)) {
                fetch("/User/Update", {
                    method: 'PATCH',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(changes)
                }).then(r => r.json()).then(console.log);
            }
        }
    }
}

function deleteProfileBtnClick() {
    if (confirm(`Delete profile:`)) {
        let login = prompt("Enter your login to confirm deletion:");
        if (login == null || login.trim() == "") {
            alert("Deletion cancelled.");
            return;
        }
        fetch("/User/Delete", {
            method: 'DELETE',
            headers: {
                'Authentication-Control': new Base64().encodeUrl(login)
            },
        }).then(r => r.json()).then(j => {
            console.log(j);
            if (j.status == 200) {
                alert("Profile deleted successfully.");
                window.location = '/';
            } else {
                alert("Error deleting profile: " + j.data);
            }
        });
    }
}