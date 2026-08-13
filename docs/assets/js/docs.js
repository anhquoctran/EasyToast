(function () {
  var sidebar = document.getElementById("sidebar");
  var btn = document.getElementById("menu-btn");
  var backdrop = document.getElementById("backdrop");

  function close() {
    sidebar.classList.remove("is-open");
    if (backdrop) backdrop.hidden = true;
  }

  function open() {
    sidebar.classList.add("is-open");
    if (backdrop) backdrop.hidden = false;
  }

  if (btn) btn.addEventListener("click", open);
  if (backdrop) backdrop.addEventListener("click", close);

  document.querySelectorAll(".sidebar a").forEach(function (a) {
    a.addEventListener("click", function () {
      if (window.matchMedia("(max-width: 900px)").matches) close();
    });
  });

  var toc = document.getElementById("api-toc");
  if (!toc) return;

  var links = Array.prototype.slice.call(toc.querySelectorAll("a[href^='#']"));
  var ids = links.map(function (a) { return a.getAttribute("href").slice(1); });

  function setCurrent() {
    var current = ids[0];
    for (var i = 0; i < ids.length; i++) {
      var el = document.getElementById(ids[i]);
      if (el && el.getBoundingClientRect().top <= 96) current = ids[i];
    }
    links.forEach(function (a) {
      a.classList.toggle("is-current", a.getAttribute("href") === "#" + current);
    });
  }

  window.addEventListener("scroll", setCurrent, { passive: true });
  setCurrent();
})();
