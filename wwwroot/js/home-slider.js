// Carousel xoay vòng seamless thật cho 3 sliders
document.addEventListener('DOMContentLoaded', function () {

    const sliders = ['hot', 'hay', 'full'];

    sliders.forEach(function (sliderName) {

        const track = document.getElementById(sliderName + 'Track');
        if (!track) return;

        const originalItems = Array.from(track.children);
        const totalItems = originalItems.length;

        // ít item quá thì không chạy
        if (totalItems < 7) return;

        const showItems = 6;
        const gap = 12;
        const itemWidth = originalItems[0].offsetWidth + gap;

        // ========================
        // Clone cuối lên đầu
        // ========================
        for (let i = totalItems - showItems; i < totalItems; i++) {
            const clone = originalItems[i].cloneNode(true);
            track.insertBefore(clone, track.firstChild);
        }

        // ========================
        // Clone đầu xuống cuối
        // ========================
        for (let i = 0; i < showItems; i++) {
            const clone = originalItems[i].cloneNode(true);
            track.appendChild(clone);
        }

        // ========================
        // Bắt đầu ở item thật đầu tiên
        // ========================
        let currentIndex = showItems;
        let isMoving = false;

        track.style.transition = 'none';
        track.style.transform =
            `translateX(-${currentIndex * itemWidth}px)`;

        // ========================
        // Hàm di chuyển
        // ========================
        function moveSlider() {
            isMoving = true;

            track.style.transition = 'transform 0.6s ease-in-out';
            track.style.transform =
                `translateX(-${currentIndex * itemWidth}px)`;
        }

        // ========================
        // Next
        // ========================
        function slideNext() {
            if (isMoving) return;

            currentIndex += showItems;
            moveSlider();
        }

        // ========================
        // Prev
        // ========================
        function slidePrev() {
            if (isMoving) return;

            currentIndex -= showItems;
            moveSlider();
        }

        // ========================
        // Khi animation xong
        // ========================
        track.addEventListener('transitionend', function () {

            isMoving = false;

            // vượt cuối
            if (currentIndex >= totalItems + showItems) {

                track.style.transition = 'none';
                currentIndex = showItems;

                track.style.transform =
                    `translateX(-${currentIndex * itemWidth}px)`;
            }

            // vượt đầu
            if (currentIndex <= 0) {

                track.style.transition = 'none';
                currentIndex = totalItems;

                track.style.transform =
                    `translateX(-${currentIndex * itemWidth}px)`;
            }
        });

        // ========================
        // Auto Slide
        // ========================
        let autoTimer;

        function startAuto() {
            clearInterval(autoTimer);
            autoTimer = setInterval(slideNext, 5000);
        }

        function stopAuto() {
            clearInterval(autoTimer);
        }

        startAuto();

        // ========================
        // Hover dừng
        // ========================
        const container = track.closest('.position-relative');

        if (container) {
            container.addEventListener('mouseenter', stopAuto);
            container.addEventListener('mouseleave', startAuto);
        }

        // ========================
        // Nút Next
        // ========================
        const nextBtn = document.querySelector(
            `[data-slider="${sliderName}"].slider-next`
        );

        nextBtn?.addEventListener('click', function () {
            stopAuto();
            slideNext();
            setTimeout(startAuto, 2500);
        });

        // ========================
        // Nút Prev
        // ========================
        const prevBtn = document.querySelector(
            `[data-slider="${sliderName}"].slider-prev`
        );

        prevBtn?.addEventListener('click', function () {
            stopAuto();
            slidePrev();
            setTimeout(startAuto, 2500);
        });

    });

});