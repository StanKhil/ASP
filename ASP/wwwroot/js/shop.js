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

function handleAddProduct(form) {
    fetch(form.action, {
        method: "POST",
        body: new FormData(form)
    }).then(r => r.json()).then(console.log).catch(console.error);
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
