// Dashboard functionality
let charts = {
    usage: null,
    height: null
};

// Get CSRF token
function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]').value;
}

// Show error message
function showAlert(message, type = 'danger', duration = 5000) {
    const alertContainer = document.getElementById('alertContainer');
    const alert = document.createElement('div');
    alert.className = `alert alert-${type} alert-dismissible fade show`;
    alert.role = 'alert';
    alert.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    alertContainer.appendChild(alert);

    if (duration) {
        setTimeout(() => alert.remove(), duration);
    }
}

// Toggle loading state
function toggleLoading(elementId, show) {
    const loader = document.getElementById(`${elementId}-loader`);
    if (loader) {
        loader.style.display = show ? 'flex' : 'none';
    }
}

// Desk control functions
async function moveDesk(deskId, direction) {
    if (!deskId) return;

    toggleLoading(`desk-${deskId}`, true);
    const handler = direction === 'up' ? 'MoveUp' : 'MoveDown';

    try {
        const response = await fetch(`?handler=${handler}&deskId=${deskId}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });

        const data = await response.json();
        if (data.success) {
            updateDeskStatus(data.desk);
            startPollingDeskStatus(deskId);
            // Record height adjustment
            const oldHeight = parseFloat(document.getElementById(`height-${deskId}`).textContent);
            await recordHeightAdjustment(deskId, oldHeight, data.desk.currentHeight);
        } else {
            showAlert(data.error || `Failed to move desk ${direction}`);
        }
    } catch (error) {
        showAlert(`Error moving desk ${direction}: ${error.message}`);
    } finally {
        toggleLoading(`desk-${deskId}`, false);
    }
}

// Update desk status
function updateDeskStatus(desk) {
    if (!desk) return;

    const heightElement = document.getElementById(`height-${desk.deskId}`);
    const progressBar = document.querySelector(`.card[data-desk-id="${desk.deskId}"] .progress-bar`);

    if (heightElement) {
        heightElement.textContent = desk.currentHeight.toFixed(1);
    }

    if (progressBar) {
        const percentage = ((desk.currentHeight - desk.minHeight) / (desk.maxHeight - desk.minHeight)) * 100;
        progressBar.style.width = `${percentage}%`;
        progressBar.setAttribute('aria-valuenow', desk.currentHeight);
    }
}

// Status polling
const deskStatusPolling = {};

function startPollingDeskStatus(deskId) {
    if (deskStatusPolling[deskId]) {
        clearInterval(deskStatusPolling[deskId]);
    }

    deskStatusPolling[deskId] = setInterval(async () => {
        try {
            const response = await fetch(`?handler=DeskStatus&deskId=${deskId}`, {
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            });
            const data = await response.json();

            if (data.success) {
                updateDeskStatus(data.desk);
                if (!data.desk.isMoving) {
                    stopPollingDeskStatus(deskId);
                }
            }
        } catch (error) {
            console.error('Error polling desk status:', error);
            stopPollingDeskStatus(deskId);
        }
    }, 1000);
}

function stopPollingDeskStatus(deskId) {
    if (deskStatusPolling[deskId]) {
        clearInterval(deskStatusPolling[deskId]);
        delete deskStatusPolling[deskId];
    }
}

// Record height adjustment
async function recordHeightAdjustment(deskId, oldHeight, newHeight) {
    try {
        await fetch(`?handler=RecordHeightAdjustment&deskId=${deskId}&oldHeight=${oldHeight}&newHeight=${newHeight}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });
    } catch (error) {
        console.error('Error recording height adjustment:', error);
    }
}

// Initialize dashboard
document.addEventListener('DOMContentLoaded', function () {
    // Initialize desk controls
    document.addEventListener('click', function (event) {
        const button = event.target.closest('button');
        if (!button) return;

        const deskCard = button.closest('.card[data-desk-id]');
        if (!deskCard) return;

        const deskId = deskCard.dataset.deskId;

        if (button.classList.contains('btn-up')) {
            event.preventDefault();
            moveDesk(deskId, 'up');
        } else if (button.classList.contains('btn-down')) {
            event.preventDefault();
            moveDesk(deskId, 'down');
        }
    });

    // Initialize refresh and toggle buttons
    document.getElementById('refreshStats')?.addEventListener('click', refreshStatistics);
    document.getElementById('toggleCharts')?.addEventListener('click', toggleChartView);

    // Initialize search
    const searchInput = document.getElementById('deskSearch');
    if (searchInput) {
        let debounceTimeout;
        searchInput.addEventListener('input', function () {
            clearTimeout(debounceTimeout);
            debounceTimeout = setTimeout(() => {
                performSearch(this.value);
            }, 300);
        });
    }

    // Load initial data
    const initialDataElement = document.getElementById('initialData');
    if (initialDataElement) {
        try {
            const desks = JSON.parse(initialDataElement.textContent);
            if (desks && desks.length > 0) {
                updateDeskList(desks);  
            }
        } catch (error) {
            console.error('Error loading initial data:', error);
            showAlert('Error loading initial data');
        }
    }
});

// Refresh statistics
async function refreshStatistics() {
    const selectedDeskElement = document.querySelector('.card[data-desk-id][data-selected-desk="true"]');
    const deskId = selectedDeskElement?.dataset.deskId;

    if (!deskId) {
        showAlert('Please select a desk first');
        return;
    }

    await loadDeskStatistics(deskId);
}

// Toggle chart view
function toggleChartView() {
    const statsSection = document.getElementById('statisticsSection');
    statsSection.classList.toggle('d-none');
}

// Load desk statistics
async function loadDeskStatistics(deskId) {
    toggleLoading('usageChart', true);
    toggleLoading('heightChart', true);

    try {
        const endDate = new Date();
        const startDate = new Date();
        startDate.setDate(startDate.getDate() - 7);

        const response = await fetch(
            `?handler=DeskUsageStatistics&deskId=${deskId}&startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`,
            {
                method: 'GET',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            }
        );

        const data = await response.json();
        if (data.success) {
            updateUsageChart(data.statistics);
            updateHeightAdjustmentsChart(data.statistics);
        } else {
            showAlert(`Error loading statistics: ${data.error}`);
        }
    } catch (error) {
        showAlert(`Failed to load statistics: ${error.message}`);
        console.error('Statistics error:', error);
    } finally {
        toggleLoading('usageChart', false);
        toggleLoading('heightChart', false);
    }
}

// Update usage chart
function updateUsageChart(statistics) {
    const ctx = document.getElementById('usageChart').getContext('2d');

    if (charts.usage) {
        charts.usage.destroy();
    }

    charts.usage = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: statistics.usageSessions.map(session =>
                new Date(session.startTime).toLocaleDateString()),
            datasets: [{
                label: 'Usage Duration (hours)',
                data: statistics.usageSessions.map(session =>
                    (new Date(session.endTime) - new Date(session.startTime)) / (1000 * 60 * 60)),
                backgroundColor: 'rgba(54, 162, 235, 0.2)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return `${context.parsed.y.toFixed(1)} hours`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Hours'
                    }
                }
            }
        }
    });
}

// Update height adjustments chart
function updateHeightAdjustmentsChart(statistics) {
    const ctx = document.getElementById('heightAdjustmentsChart').getContext('2d');

    if (charts.height) {
        charts.height.destroy();
    }

    charts.height = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: Array.from({ length: 24 }, (_, i) => `${i}:00`),
            datasets: [{
                label: 'Number of Adjustments',
                data: statistics.numberOfAdjustments,
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return `${context.parsed.y} adjustments`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1
                    },
                    title: {
                        display: true,
                        text: 'Number of Adjustments'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Hour of Day'
                    }
                }
            }
        }
    });
}

// Select desk and optionally load its statistics
function selectDesk(deskId, loadStats = true) {
    const selectedDeskLabel = document.getElementById('selectedDeskLabel');
    
    document.querySelectorAll('.card[data-desk-id]').forEach(card => {
        if (card.dataset.deskId === deskId) {
            card.dataset.selectedDesk = 'true';
            if (selectedDeskLabel) {
                const deskName = card.querySelector('.card-title')?.textContent.trim() || 'Unknown Desk';
                selectedDeskLabel.textContent = `Currently viewing: ${deskName}`;
            }
        } else {
            delete card.dataset.selectedDesk;
        }
    });

    if (loadStats) {
        loadDeskStatistics(deskId);
    }
}

// Perform search
async function performSearch(query) {
    toggleLoading('deskList', true);

    try {
        const response = await fetch(`?handler=Search&searchQuery=${encodeURIComponent(query)}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });

        const data = await response.json();
        if (data.success) {
            updateDeskList(data.results, false);  // Pass false to prevent auto-selection
        } else {
            showAlert(`Error searching desks: ${data.error}`);
        }
    } catch (error) {
        showAlert(`Failed to search desks: ${error.message}`);
        console.error('Search error:', error);
    } finally {
        toggleLoading('deskList', false);
    }
}

// Update desk list
function updateDeskList(desks, autoSelect = true) {
    const deskList = document.getElementById('deskList');
    if (!deskList) return;

    // Remember currently selected desk
    const currentSelectedDeskId = document.querySelector('.card[data-selected-desk="true"]')?.dataset.deskId;

    // Clear existing desks
    deskList.innerHTML = '';

    // Add new desks
    desks.forEach(desk => {
        const deskElement = createDeskElement(desk);
        deskList.appendChild(deskElement);
    });

    // Restore selection or auto-select first desk
    if (currentSelectedDeskId) {
        const previouslySelectedDesk = document.querySelector(`.card[data-desk-id="${currentSelectedDeskId}"]`);
        if (previouslySelectedDesk) {
            selectDesk(currentSelectedDeskId, false);  // Pass false to prevent chart refresh
            return;
        }
    }

    if (autoSelect && desks.length > 0) {
        selectDesk(desks[0].deskId);
    }
}

// Create desk element
function createDeskElement(desk) {
    const div = document.createElement('div');
    div.className = 'col-md-6 mb-4';
    div.innerHTML = `
        <div class="card" data-desk-id="${desk.deskId}">
            <div class="card-body">
                <div class="loading-overlay" id="desk-${desk.deskId}-loader" style="display: none;">
                    <div class="loading-spinner" role="status">
                        <span class="visually-hidden">Loading desk data...</span>
                    </div>
                </div>
                <h5 class="card-title">${desk.name}</h5>
                <p class="card-text">ID: ${desk.deskId}</p>
                <div class="desk-status mb-3">
                    <p class="mb-1">Current Height: <span id="height-${desk.deskId}">${desk.currentHeight.toFixed(1)}</span> cm</p>
                    <div class="progress">
                        <div class="progress-bar" role="progressbar"
                            style="width: ${((desk.currentHeight - desk.minHeight) / (desk.maxHeight - desk.minHeight)) * 100}%"
                            aria-valuenow="${desk.currentHeight}"
                            aria-valuemin="${desk.minHeight}"
                            aria-valuemax="${desk.maxHeight}">
                        </div>
                    </div>
                </div>
                <div class="desk-controls">
                    <div class="btn-group w-100">
                        <button class="btn btn-primary btn-up">
                            <i class="fas fa-arrow-up"></i><span class="ms-2">Up</span>
                        </button>
                        <button class="btn btn-primary btn-down">
                            <i class="fas fa-arrow-down"></i><span class="ms-2">Down</span>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    return div;
}

// Load initial desks
async function loadInitialDesks() {
    toggleLoading('deskList', true);

    try {
        const response = await fetch(`?handler=GetAllDesksAsync`, {
            method: 'GET',
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });

        const data = await response.json();
        if (data.success) {
            updateDeskList(data.desks);
        } else {
            showAlert(`Error loading desks: ${data.error}`);
        }
    } catch (error) {
        showAlert(`Failed to load desks: ${error.message}`);
        console.error('Error loading desks:', error);
    } finally {
        toggleLoading('deskList', false);
    }
}
