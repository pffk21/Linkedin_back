class Base64 {
    static #textEncoder = new TextEncoder();
    static #textDecoder = new TextDecoder();

    // https://datatracker.ietf.org/doc/html/rfc4648#section-4
    static encode = (str) => btoa(String.fromCharCode(...Base64.#textEncoder.encode(str)));
    static decode = (str) => Base64.#textDecoder.decode(Uint8Array.from(atob(str), c => c.charCodeAt(0)));

    // https://datatracker.ietf.org/doc/html/rfc4648#section-5
    static encodeUrl = (str) => this.encode(str).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
    static decodeUrl = (str) => this.decode(str.replace(/\-/g, '+').replace(/\_/g, '/'));

    static jwtEncodeBody = (header, payload) => this.encodeUrl(JSON.stringify(header)) + '.' + this.encodeUrl(JSON.stringify(payload));
    static jwtDecodePayload = (jwt) => JSON.parse(this.decodeUrl(jwt.split('.')[1]));
}

document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id == 'auth-form') {
        e.preventDefault();
        const formData = new FormData(form);
        const login = formData.get("user-login");
        const password = formData.get("user-password");

        // https://datatracker.ietf.org/doc/html/rfc7617#section-2
        const userPass = `${login}:${password}`;
        const credentials = Base64.encode(userPass);
        console.log(login, password, credentials);
        fetch("/api/user", {
            method: 'GET',
            headers: {
                'Authorization': `Basic ${credentials}`
            }
        }).then(r => r.json())
            .then(j => {
                if (typeof j.status == 'undefined') {
                    window.location.reload();
                }
            })
            .catch(console.error);


    }
    if (form.id == 'admin-group-form') {
        e.preventDefault();
        fetch("/api/group", {
            method: 'POST',
            body: new FormData(form),
        }).then(r => r.json()).then(j => {
            alert(j.status);
            if (j.code == 200) {
                form.reset();
            }
        })
    }
    if (form.id == 'admin-product-form') {
        e.preventDefault();

        const formData = new FormData(form);
        const groupId = formData.get("product-group-id")?.trim();
        const name = formData.get("product-name")?.trim();
        const description = formData.get("product-description")?.trim();
        const slug = formData.get("product-slug")?.trim();
        const price = parseFloat(formData.get("product-price"));
        const stock = parseInt(formData.get("product-stock"));

        if (!groupId) {
            alert("Виберіть групу товару");
            return;
        }
        if (!name) {
            alert("Назва обов'язкова");
            return;
        }
        if (name.length > 100) {
            alert("Назва не може бути довше 100 символів");
            return;
        }
        if (description && description.length > 500) {
            alert("Опис не може бути довше 500 символів");
            return;
        }
        if (slug && !/^[a-z0-9-]+$/.test(slug)) {
            alert("Slug може містити лише латинські малі букви, цифри та дефіс (-)");
            return;
        }
        if (isNaN(price) || price <= 0) {
            alert("Ціна має бути більше 0");
            return;
        }
        if (isNaN(stock) || stock < 0) {
            alert("Кількість на складі не може бути від’ємною");
            return;
        }

        fetch("/api/product", {
            method: 'POST',
            body: formData,
        }).then(r => r.json()).then(j => {
            alert(j.status);
            if (j.code == 200) {
                form.reset();
            }
        });
    }
});

//document.getElementById("registrationForm").addEventListener("submit", async function (e) {
//    e.preventDefault();

//    const formData = new FormData(this);
//    const data = {
//        FirstName: formData.get("FirstName"),
//        LastName: formData.get("LastName"),
//        Email: formData.get("Email"),
//        WorkingPlace: formData.get("WorkingPlace"),
//        Location: formData.get("Location"),
//        ProfileImageUrl: formData.get("ProfileImageUrl"),
//        Password: formData.get("Password"),
//        ConfirmPassword: formData.get("ConfirmPassword")
//    };

//    const response = await fetch("/api/user/registration", {
//        method: "POST",
//        headers: { "Content-Type": "application/json" },
//        body: JSON.stringify(data)
//    });

//    const result = await response.json();
//    alert(result.status || result.message);
//});
