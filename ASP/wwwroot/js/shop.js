document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id == "product-add-form") {
        e.preventDefault();
        handleAddProduct(form)
    }
    if (form.id == "group-add-form") {
        e.preventDefault();
        handleAddGroup(form)
    }
});

document.addEventListener("DOMContentLoaded", () => {
    for (let btn of document.querySelectorAll("[data-product-id]")) {
        btn.addEventListener("click", addToCartClick);
    }
});

function addToCartClick(e) {
    const btn = e.target.closest("[data-product-id]");
    if (!btn) throw "closest failed '[data-product-id]'";
    const productId = btn.getAttribute("data-product-id");

    fetch(`/api/cart/${productId}`, { method: "POST" })
        .then(async (r) => {
            if (r.ok) {
                const data = await r.json();
                alert("Товар додано до кошика!");
            } else if (r.status === 401) {
                if (confirm("Для роботи з кошиком потрібно увійти. Хочете авторизуватися?")) {
                    const page = document.getElementById("authModal");
                    const modal = new bootstrap.Modal(page);
                    modal.show();
                }
            } else {
                if (confirm("Виникла помилка, оновити сторінку?")) {
                    location.reload();
                }
            }
        })
        .catch((err) => {
            if (confirm("Виникла помилка, оновити сторінку?")) {
                location.reload();
            }
        });
}


function handleAddProduct(form) {
    fetch(form.action, {
        method: "POST",
        body: new FormData(form)
    })
        .then(r => r.json())
        .then(data => {
            alert(data.name);

            if (data.status === 201) {
                form.reset();
            }
        })
        .catch(err => {
            console.error(err);
            alert("Unexpected error");
        });
}

function handleAddGroup(form) {
    fetch(form.action, {
        method: "POST",
        body: new FormData(form)
    })
        .then(r => r.json())
        .then(data => {
            alert(data.name);

            if (data.status === 201) {
                form.reset();
            }
        })
        .catch(err => {
            console.error(err);
            alert("Unexpected error");
        });
}

