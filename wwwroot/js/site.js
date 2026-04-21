// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.documentElement.style.scrollBehavior = "smooth";

/* REVEAL */
const observer = new IntersectionObserver(entries => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('show');
        }
    });
});
document.querySelectorAll('.reveal').forEach(el => observer.observe(el));

/* PARALLAX */
window.addEventListener("scroll", () => {
    const scroll = window.scrollY;
    document.querySelectorAll("img").forEach(img => {
        img.style.transform = `translateY(${scroll * 0.03}px)`;
    });
});

/* CARD 3D */
document.querySelectorAll('.feature-card').forEach(card => {
    let rect;

    card.addEventListener('mouseenter', () => {
        rect = card.getBoundingClientRect();
    });

    card.addEventListener('mousemove', e => {
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        const rotateX = ((y / rect.height) - 0.5) * -10;
        const rotateY = ((x / rect.width) - 0.5) * 10;

        card.style.transform = `rotateX(${rotateX}deg) rotateY(${rotateY}deg) scale(1.03)`;
    });

    card.addEventListener('mouseleave', () => {
        card.style.transform = 'rotateX(0) rotateY(0) scale(1)';
    });
});

/* BUTTON MAGNET */
document.querySelectorAll(".btn-main").forEach(btn => {
    btn.addEventListener("mousemove", e => {
        const rect = btn.getBoundingClientRect();

        const x = (e.clientX - rect.left - rect.width / 2) * 0.15;
        const y = (e.clientY - rect.top - rect.height / 2) * 0.15;

        btn.style.transform = `translate(${x}px, ${y}px)`;
    });

    btn.addEventListener("mouseleave", () => {
        btn.style.transform = "translate(0,0)";
    });
});

/* CURSOR GLOW */
const glow = document.querySelector(".cursor-glow");

document.addEventListener("mousemove", e => {
    if (glow) {
        glow.style.left = e.clientX + "px";
        glow.style.top = e.clientY + "px";
    }
});
/* CONNECTION OBSERVER */
const connectionSections = document.querySelectorAll('.connection-section');

const connectionObserver = new IntersectionObserver(entries => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('show');
        }
    });
}, { threshold: 0.3 });

connectionSections.forEach(el => connectionObserver.observe(el));


/* DASHBOARD MOVE WITH LINE */
window.addEventListener("scroll", () => {

    connectionSections.forEach(section => {

        const rect = section.getBoundingClientRect();
        const offset = rect.top * 0.04;

        const card = section.querySelector('.dashboard-card');

        if (card) {
            card.style.transform = `translateY(${offset}px)`;
        }

    });

});




