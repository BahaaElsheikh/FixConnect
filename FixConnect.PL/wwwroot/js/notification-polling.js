(function () {
    function updateBadge(elementId, count) {
        var el = document.getElementById(elementId);
        if (!el) return;

        if (count > 0) {
            el.textContent = count;
            el.classList.remove("hidden");
            el.classList.add(
                "ml-auto", "inline-flex", "items-center", "justify-center",
                "min-w-[20px]", "h-5", "px-1.5", "rounded-full",
                "bg-error", "text-on-error", "text-[11px]", "font-bold", "leading-none"
            );
        } else {
            el.textContent = "";
            el.classList.add("hidden");
        }
    }

    function pollNotificationCounts() {
        fetch("/Worker/GetNotificationCounts")
            .then(function (response) {
                if (!response.ok) throw new Error("Failed to fetch notification counts");
                return response.json();
            })
            .then(function (data) {
                updateBadge("badge-direct-requests", data.directRequests);
                updateBadge("badge-proposals", data.proposals);
                updateBadge("badge-jobs", data.jobs);
                updateBadge("badge-wallet", data.wallet);
            })
            .catch(function (err) {
                console.warn("Notification polling error:", err);
            });
    }

    document.addEventListener("DOMContentLoaded", function () {
        pollNotificationCounts(); // initial load
        setInterval(pollNotificationCounts, 10000); // every 10s
    });
})();