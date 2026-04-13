(function () {
	const App = {
		overlay: document.getElementById("globalLoadingOverlay"),

		showLoading() {
			if (this.overlay) {
				this.overlay.classList.remove("d-none");
				this.overlay.setAttribute("aria-hidden", "false");
			}
		},

		hideLoading() {
			if (this.overlay) {
				this.overlay.classList.add("d-none");
				this.overlay.setAttribute("aria-hidden", "true");
			}
		},

		initToasts() {
			if (!window.bootstrap) {
				return;
			}

			document.querySelectorAll(".toast[data-autoshow='true']").forEach((element) => {
				const toast = new bootstrap.Toast(element);
				toast.show();
			});
		},

		initRevealAnimation() {
			const reveals = document.querySelectorAll(".reveal-up");
			if (!reveals.length || !window.IntersectionObserver) {
				reveals.forEach((node) => node.classList.add("in-view"));
				return;
			}

			const observer = new IntersectionObserver((entries) => {
				entries.forEach((entry) => {
					if (entry.isIntersecting) {
						entry.target.classList.add("in-view");
						observer.unobserve(entry.target);
					}
				});
			}, { threshold: 0.2 });

			reveals.forEach((node, index) => {
				node.style.transitionDelay = `${Math.min(index * 60, 260)}ms`;
				observer.observe(node);
			});
		},

		initHeroTabs() {
			const tabs = document.querySelectorAll(".ride-tab");
			const scheduleFields = document.getElementById("scheduleFields");

			tabs.forEach((tab) => {
				tab.addEventListener("click", () => {
					tabs.forEach((item) => item.classList.remove("active"));
					tab.classList.add("active");

					if (scheduleFields) {
						scheduleFields.hidden = tab.dataset.tab !== "later";
					}
				});
			});
		},

		debounce(callback, wait = 300) {
			let timeoutId;
			return (...args) => {
				window.clearTimeout(timeoutId);
				timeoutId = window.setTimeout(() => callback(...args), wait);
			};
		}
	};

	window.VehicleBookingApp = App;

	document.addEventListener("DOMContentLoaded", () => {
		App.initToasts();
		App.initRevealAnimation();
		App.initHeroTabs();
	});
})();
