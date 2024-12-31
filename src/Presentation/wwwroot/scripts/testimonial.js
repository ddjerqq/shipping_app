window.initializeInfiniteMovingCards = (testimonials) => {
    const container = document.getElementById('infiniteMovingCards');
    const speed = 'slow'; 
    const direction = 'right'; 
    testimonials.forEach(item => {
        const card = document.createElement('div');
        card.classList.add('card');
        card.innerHTML = `
            <div class="quote">${item.quote}</div>
            <div class="author">${item.name}, ${item.title}</div>
        `;
        container.appendChild(card);
    });

    let currentX = 0;
    const moveSpeed = speed === 'slow' ? 1 : 5; 

    const moveCards = () => {
        const cards = document.querySelectorAll('.card');
        cards.forEach(card => {
            currentX -= moveSpeed;
            card.style.transform = `translateX(${currentX}px)`;
            if (Math.abs(currentX) > card.offsetWidth) {
                currentX = 0;
                card.style.transition = 'none';
                setTimeout(() => {
                    card.style.transition = 'transform 0.5s ease-out';
                }, 50);
            }
        });
        requestAnimationFrame(moveCards);
    };

    moveCards();
};
