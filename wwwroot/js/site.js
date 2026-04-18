document.addEventListener('DOMContentLoaded', function() {
    // Slider functionality
    const sliders = ['hot', 'hay', 'full'];
    
    sliders.forEach(sliderId => {
        const track = document.getElementById(sliderId + 'Track');
        const prevBtn = document.querySelector(`[data-slider="${sliderId}"].slider-prev`);
        const nextBtn = document.querySelector(`[data-slider="${sliderId}"].slider-next`);
        const container = track.parentElement;
        
        if (!track || !prevBtn || !nextBtn) return;
        
        let currentTranslate = 0;
        let translateAmount = 0;
        const cards = track.children.length;
        const cardWidth = container.offsetWidth / 6 + 24; // 6 cards + margin
        
        function updateSlider() {
            if (translateAmount > 0) translateAmount = 0;
            if (translateAmount < -(cards - 6) * cardWidth) {
                translateAmount = -(cards - 6) * cardWidth;
            }
            track.style.transform = `translateX(${translateAmount}px)`;
        }
        
        prevBtn.addEventListener('click', () => {
            translateAmount += cardWidth;
            updateSlider();
        });
        
        nextBtn.addEventListener('click', () => {
            translateAmount -= cardWidth;
            updateSlider();
        });
    });
});
