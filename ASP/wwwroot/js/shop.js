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
    for (let btn of document.querySelectorAll("[data-cart-product-id]")) {
        btn.onclick = modifyCartQuantity;
    }

    for (let btn of document.querySelectorAll("[data-cart-product-id")) {
        btn.onclick = removeFromCart;
    }

    let btn = document.getElementById("discard-cart");
    if (btn) btn.onclick = discardCartClick;
    btn = document.getElementById("checkout-cart");
    if (btn) btn.onclick = checkoutCartClick;
});

function modifyCartQuantity(e) {
    const btn = e.target.closest("[data-cart-product-id]");
    if (!btn) throw `modifyCartQuantity: closest("[data-cart-product-id]") not found`;
    const productId = btn.getAttribute("data-cart-product-id");
    const increment = btn.getAttribute("data-increment");
    fetch(`/api/cart/${productId}?increment=${increment}`, {
        method: 'PATCH'
    }).then(r => r.json()).then(j => {
        if (j.status.isOk) {
            window.location.reload();
            console.log(j);
        }
        else {
            alarm(j.data);
        }
    });
}

function removeFromCart(e) {
    if (!confirm("Видалити товар з кошика?")) return;
    console.log(e.currentTarget);
    target = e.currentTarget;
    if (!target) target = e.target;
    fetch(`/api/cart/${target.getAttribute("data-cart-product-id") }`, {
        method: 'DELETE'
    }).then(r => r.json()).then(j => {
        if (j.status.isOk) {
            window.location.reload();
            console.log(j);
        }
        else {
            alarm(j.data);
        }
    });
}


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

function discardCartClick() {
    const count = document
        .getElementById("total-count")
        .getAttribute("data-cart-total");
    const price = document
        .getElementById("total-price")
        .getAttribute("data-cart-total");

    if (confirm(`Ви впевнені, що хочете видалити кошик з ${count} товарів та сумою ${price} грн?`)) {
        fetch("/api/cart", {
            method: "DELETE"
        })
            .then(r => r.json()).then(j => {
                if (j.status.isOk) {
                    alarm("Cart discarded");
                    window.location = "/Shop";
                }
                else {
                    alarm(j.data);
                }
            });
    }
}

function checkoutCartClick() {
    const count = document
        .getElementById("total-count")
        .getAttribute("data-cart-total");
    const price = document
        .getElementById("total-price")
        .getAttribute("data-cart-total");
    if (confirm(`Ви підтверджуєте заповлення на ${count} товари та загальною ціною ${price}`)) {
        fetch("/api/cart", {
            method: "PUT"
        }).then(r => r.json()).then(j => {
            if (j.status.isOk) {
                alarm("Congratulations! Order completed.");
                window.location = "/Shop";
            }
            else {
                alarm(j.data);
            }
        });
    }
    
}

function alarm(msg) {
    alert(msg);
}
