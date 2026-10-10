(function () {
    'use strict';

    var STORAGE_KEY = 'idp-sidebar-collapsed';

    function initSidebar() {
        var toggleBtn = document.getElementById('sidebar-toggle');
        var page = document.querySelector('.page');

        if (!toggleBtn || !page) {
            return;
        }

        if (localStorage.getItem(STORAGE_KEY) === 'true') {
            page.classList.add('sidebar-collapsed');
            toggleBtn.setAttribute('aria-expanded', 'false');
        } else {
            toggleBtn.setAttribute('aria-expanded', 'true');
        }

        toggleBtn.addEventListener('click', function () {
            var isCollapsed = page.classList.toggle('sidebar-collapsed');
            toggleBtn.setAttribute('aria-expanded', isCollapsed ? 'false' : 'true');
            localStorage.setItem(STORAGE_KEY, isCollapsed ? 'true' : 'false');
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSidebar);
    } else {
        initSidebar();
    }
}());
