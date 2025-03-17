// Obtén los elementos del modal
var modal = document.getElementById("imageModal");
var modalImg = document.getElementById("imgModal");
var captionText = document.getElementById("caption");
var currentIndex = 0;

// Selecciona solo las imágenes principales dentro de la galería
var images = document.querySelectorAll(".galeria-port .imagen-port .main-image");

// Muestra la imagen en el modal al hacer clic
images.forEach(function(img, index) {
  img.addEventListener("click", function() {
    modal.style.display = "block";
    modalImg.src = img.dataset.modalImage || img.src;  // Usa la URL desde el atributo data-modal-image o el src
    captionText.innerHTML = img.alt;
    currentIndex = index;
  });
});

// Cerrar el modal
var closeBtn = document.getElementsByClassName("close")[0];
closeBtn.onclick = function() {
  modal.style.display = "none";
}

// Navegación entre imágenes (si quieres la funcionalidad de anterior/siguiente)
var prevBtn = document.getElementById("prev");
var nextBtn = document.getElementById("next");

prevBtn.onclick = function() {
  showImage(currentIndex - 1);
};

nextBtn.onclick = function() {
  showImage(currentIndex + 1);
};

// Función para mostrar una imagen específica en el modal (para la navegación)
function showImage(index) {
  if (index >= images.length) {
    currentIndex = 0;
  } else if (index < 0) {
    currentIndex = images.length - 1;
  } else {
    currentIndex = index;
  }
  modalImg.src = images[currentIndex].dataset.modalImage || images[currentIndex].src;
  captionText.innerHTML = images[currentIndex].alt;
}
