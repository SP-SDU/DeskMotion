// Dashboard functionality
let charts = {
    usage: null,
    height: null
};

// Get antiforgery token
function getAntiForgeryToken() {
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenElement ? tokenElement.value : null;
    console.log('Retrieved antiforgery token:', token ? 'Token found' : 'Token not found');
    return token;
}

// Show error message
function showAlert(message, type = 'danger', duration = 5000) {
    console.log('Showing alert:', message);
    const alertContainer = document.getElementById('alertContainer');
    if (!alertContainer) {
        console.error('Alert container not found');
        return;
    }

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
    console.log('Toggling loading state for:', elementId);
    const loader = document.getElementById(`${elementId}-loader`);
    if (loader) {
        loader.style.display = show ? 'flex' : 'none';
    }
}

// Desk control functions
async function moveDesk(deskId, direction) {
    console.log('moveDesk called with ID:', deskId, 'and direction:', direction);
    if (!deskId) {
        console.error('Invalid desk ID');
        return;
    }

    toggleLoading(`desk-${deskId}`, true);
    const handler = direction === 'up' ? 'MoveUp' : 'MoveDown';

    try {
        console.log('Sending move desk request');
        const response = await fetch(`?handler=${handler}&deskId=${deskId}`, {
            method: 'POST'
        });

        console.log('Received response:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('Response data:', data);
        
        if (data.success) {
            console.log('Desk moved successfully');
            updateDeskStatus(data.desk);
            startPollingDeskStatus(deskId);
            // Record height adjustment
            const oldHeight = data.desk.previousHeight;
            await recordHeightAdjustment(deskId, oldHeight, data.desk.currentHeight);
        } else {
            console.error('Failed to move desk:', data.error);
            showAlert(data.error || `Failed to move desk ${direction}`);
        }
    } catch (error) {
        console.error('Error in moveDesk:', error);
        showAlert(`Error moving desk ${direction}: ${error.message}`);
    } finally {
        toggleLoading(`desk-${deskId}`, false);
    }
}

// Update desk status
function updateDeskStatus(desk) {
    console.log('Updating desk status for:', desk.deskId);
    if (!desk) {
        console.error('Invalid desk data');
        return;
    }

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
    console.log('Starting polling for desk:', deskId);
    if (deskStatusPolling[deskId]) {
        clearInterval(deskStatusPolling[deskId]);
    }

    deskStatusPolling[deskId] = setInterval(async () => {
        try {
            console.log('Sending desk status request');
            const response = await fetch(`?handler=DeskStatus&deskId=${deskId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            console.log('Received response:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            console.log('Response data:', data);
            
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
    console.log('Stopping polling for desk:', deskId);
    if (deskStatusPolling[deskId]) {
        clearInterval(deskStatusPolling[deskId]);
        delete deskStatusPolling[deskId];
    }
}

// Record height adjustment
async function recordHeightAdjustment(deskId, oldHeight, newHeight) {
    console.log('Recording height adjustment for desk:', deskId);
    try {
        console.log('Sending record height adjustment request');
        const response = await fetch(`?handler=RecordHeightAdjustment&deskId=${deskId}&oldHeight=${oldHeight}&newHeight=${newHeight}`, {
            method: 'POST'
        });
    } catch (error) {
        console.error('Error recording height adjustment:', error);
    }
}

// Initialize dashboard
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM Content Loaded - Initializing dashboard');
    
    // Initialize desk controls
    document.addEventListener('click', async function (event) {
        console.log('Click event captured on:', event.target);
        const target = event.target;

        // Handle assign desk button clicks
        if (target.closest('.assign-desk-btn')) {
            console.log('Assign desk button clicked');
            const button = target.closest('.assign-desk-btn');
            const deskId = button.dataset.deskId;
            console.log('Desk ID from button:', deskId);
            await assignDesk(deskId);
            return;
        }

        // Handle unassign desk button clicks
        if (target.closest('.unassign-desk-btn')) {
            console.log('Unassign desk button clicked');
            const button = target.closest('.unassign-desk-btn');
            const deskId = button.dataset.deskId;
            console.log('Desk ID from button:', deskId);
            await unassignDesk(deskId);
            return;
        }

        // Handle up/down button clicks
        if (target.closest('.move-up-btn, .move-down-btn')) {
            console.log('Move up/down button clicked');
            const button = target.closest('.move-up-btn, .move-down-btn');
            const direction = button.classList.contains('move-up-btn') ? 'up' : 'down';
            const deskId = button.dataset.deskId;
            console.log('Moving desk:', deskId, 'direction:', direction);
            await moveDesk(deskId, direction);
            return;
        }
    });

    // Load initial desks
    loadInitialDesks();
});

// Assign desk to current user
async function assignDesk(deskId) {
    console.log('assignDesk called with ID:', deskId);
    
    if (!deskId) {
        console.error('Invalid desk ID');
        showAlert('Invalid desk ID', 'danger');
        return;
    }

    const deskElement = document.querySelector(`[data-desk-id="${deskId}"]`);
    if (!deskElement) {
        console.error('Desk element not found');
        return;
    }

    try {
        // Optimistically update UI
        console.log('Optimistically updating UI for desk assignment');
        const deskName = deskElement.querySelector('.card-title').textContent.trim();
        const timestamp = new Date().toISOString();
        
        // Move desk to assigned section immediately
        moveDeskToAssigned({
            id: deskId,
            name: deskName,
            assignmentStart: timestamp
        });

        // Show loading state
        showAlert('Assigning desk...', 'info');

        // Send request to backend
        console.log('Sending assign desk request');
        const token = getAntiForgeryToken();
        const response = await fetch('?handler=AssignDesk', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ deskId })
        });

        console.log('Received response:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        console.log('Response data:', result);
        
        if (result.success) {
            console.log('Desk assigned successfully');
            showAlert('Desk assigned successfully!', 'success');
            
            // Refresh the desk lists to ensure consistency
            await loadInitialDesks();
        } else {
            console.error('Failed to assign desk:', result.error);
            showAlert(result.error || 'Failed to assign desk', 'danger');
            // Revert optimistic update
            await loadInitialDesks();
        }
    } catch (error) {
        console.error('Error in assignDesk:', error);
        showAlert('An error occurred while assigning the desk', 'danger');
        // Revert optimistic update
        await loadInitialDesks();
    }
}

// Unassign desk from current user
async function unassignDesk(deskId) {
    console.log('unassignDesk called with ID:', deskId);
    
    if (!deskId) {
        console.error('Invalid desk ID');
        showAlert('Invalid desk ID', 'danger');
        return;
    }

    const deskElement = document.querySelector(`[data-desk-id="${deskId}"]`);
    if (!deskElement) {
        console.error('Desk element not found');
        return;
    }

    try {
        // Optimistically update UI
        console.log('Optimistically updating UI for desk unassignment');
        const deskName = deskElement.querySelector('.card-title').textContent.trim();
        
        // Move desk to available section immediately
        moveDeskToAvailable({
            id: deskId,
            name: deskName
        });

        // Show loading state
        showAlert('Unassigning desk...', 'info');

        // Send request to backend
        console.log('Sending unassign desk request');
        const token = getAntiForgeryToken();
        const response = await fetch('?handler=UnassignDesk', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ deskId })
        });

        console.log('Received response:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        console.log('Response data:', result);
        
        if (result.success) {
            console.log('Desk unassigned successfully');
            showAlert('Desk unassigned successfully!', 'success');
            
            // Refresh the desk lists to ensure consistency
            await loadInitialDesks();
        } else {
            console.error('Failed to unassign desk:', result.error);
            showAlert(result.error || 'Failed to unassign desk', 'danger');
            // Revert optimistic update
            await loadInitialDesks();
        }
    } catch (error) {
        console.error('Error in unassignDesk:', error);
        showAlert('An error occurred while unassigning the desk', 'danger');
        // Revert optimistic update
        await loadInitialDesks();
    }
}

// Helper function to move desk to assigned section
function moveDeskToAssigned(desk) {
    console.log('Moving desk to assigned section:', desk);
    
    // Remove from available section if it exists
    const availableDeskElement = document.querySelector(`#availableDeskList [data-desk-id="${desk.id}"]`);
    if (availableDeskElement) {
        availableDeskElement.remove();
    }

    // Create assigned desk element
    const assignedDeskList = document.querySelector('#assignedDeskList');
    if (!assignedDeskList) {
        // Create the list if it doesn't exist
        const assignedDesksContainer = document.querySelector('#assignedDesks');
        if (!assignedDesksContainer) {
            console.error('Could not find assigned desks container');
            return;
        }
        assignedDesksContainer.innerHTML = '<ul class="desk-list" id="assignedDeskList"></ul>';
    }

    // Create the desk list item
    const li = document.createElement('li');
    li.className = 'desk-list-item';
    li.setAttribute('data-desk-id', desk.id);
    
    li.innerHTML = `
        <div class="desk-name">${desk.name}</div>
        <div class="desk-status">
            <i class="fa-solid fa-circle text-success"></i>
            <span>Active</span>
        </div>
        <div class="desk-status">
            <i class="fa-solid fa-clock"></i>
            <span>Since: ${new Date(desk.assignmentStart).toLocaleString()}</span>
        </div>
        <button class="btn btn-sm btn-danger mt-2" onclick="unassignDesk('${desk.id}')">
            <i class="fa-solid fa-times"></i>
            <span>Release Desk</span>
        </button>
    `;

    // Add to assigned section
    document.querySelector('#assignedDeskList').appendChild(li);
    
    // Remove no desks message if it exists
    const noDesksMessage = document.querySelector('#assignedDesks .no-desks-message');
    if (noDesksMessage) {
        noDesksMessage.remove();
    }
    
    // Update counters
    updateDeskCounters();
}

// Helper function to move desk to available section
function moveDeskToAvailable(desk) {
    console.log('Moving desk to available section:', desk);
    
    // Remove from assigned section if it exists
    const assignedDeskElement = document.querySelector(`#assignedDeskList [data-desk-id="${desk.id}"]`);
    if (assignedDeskElement) {
        assignedDeskElement.remove();
    }

    // Create available desk element
    const availableDeskList = document.querySelector('#availableDeskList');
    if (!availableDeskList) {
        // Create the list if it doesn't exist
        const availableDesksContainer = document.querySelector('#availableDesks');
        if (!availableDesksContainer) {
            console.error('Could not find available desks container');
            return;
        }
        availableDesksContainer.innerHTML = '<ul class="desk-list" id="availableDeskList"></ul>';
    }

    // Create the desk list item
    const li = document.createElement('li');
    li.className = 'desk-list-item';
    li.setAttribute('data-desk-id', desk.id);
    
    li.innerHTML = `
        <div class="desk-name">${desk.name}</div>
        <div class="desk-status">
            <i class="fa-solid fa-circle text-success"></i>
            <span>Available</span>
        </div>
        <button class="btn btn-sm btn-primary assign-desk-btn" data-desk-id="${desk.id}">
            <i class="fa-solid fa-plus"></i>
            <span>Assign to Me</span>
        </button>
    `;

    // Add to available section
    document.querySelector('#availableDeskList').appendChild(li);
    
    // Remove no desks message if it exists
    const noDesksMessage = document.querySelector('#availableDesks .no-desks-message');
    if (noDesksMessage) {
        noDesksMessage.remove();
    }
    
    // Update counters
    updateDeskCounters();
}

// Helper function to update desk counters
function updateDeskCounters() {
    const assignedCount = document.querySelectorAll('#assignedDeskList .desk-list-item').length;
    const availableCount = document.querySelectorAll('#availableDeskList .desk-list-item').length;
    
    const assignedTitle = document.querySelector('.sidebar-section:first-child h2');
    const availableTitle = document.querySelector('.sidebar-section:last-child h2');
    
    if (assignedTitle) {
        assignedTitle.textContent = `Assigned Desks (${assignedCount})`;
    }
    
    if (availableTitle) {
        availableTitle.textContent = `Available Desks (${availableCount})`;
    }
}

// Update both assigned and available desk lists
function updateDeskLists(desks) {
    console.log('Updating desk lists with:', desks);
    
    // Get containers
    const assignedDesksContainer = document.querySelector('#assignedDesks');
    const availableDesksContainer = document.querySelector('#availableDesks');
    
    // Clear existing content
    assignedDesksContainer.innerHTML = '';
    availableDesksContainer.innerHTML = '';
    
    // Split desks into assigned and available
    const assignedDesks = desks.filter(desk => desk.isAssigned);
    const availableDesks = desks.filter(desk => !desk.isAssigned);
    
    console.log('Assigned desks:', assignedDesks);
    console.log('Available desks:', availableDesks);

    // Update assigned desks section
    if (assignedDesks.length === 0) {
        assignedDesksContainer.innerHTML = `
            <div class="no-desks-message">
                No assigned desks yet. Select desk below.
            </div>
        `;
    } else {
        assignedDesks.forEach(desk => {
            const deskElement = createDeskElement(desk);
            assignedDesksContainer.appendChild(deskElement);
        });
    }

    // Update available desks section
    if (availableDesks.length === 0) {
        availableDesksContainer.innerHTML = `
            <div class="no-desks-message">
                No available desks at the moment.
            </div>
        `;
    } else {
        availableDesks.forEach(desk => {
            const deskElement = createDeskElement(desk);
            availableDesksContainer.appendChild(deskElement);
        });
    }
}

// Create desk element
function createDeskElement(desk) {
    const div = document.createElement('div');
    div.className = 'desk-item card mb-3';
    div.setAttribute('data-desk-id', desk.id);
    
    const assignButton = desk.isAssigned
        ? `<button class="btn btn-sm btn-danger unassign-desk-btn" data-desk-id="${desk.id}">
            <span>Unassign</span>
           </button>`
        : `<button class="btn btn-sm btn-primary assign-desk-btn" data-desk-id="${desk.id}">
            <span>Assign to Me</span>
           </button>`;
    
    div.innerHTML = `
        <div class="card-body">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <h5 class="card-title mb-1">${desk.name}</h5>
                    <p class="card-text mb-1">
                        <small class="text-muted">
                            Height: ${desk.currentHeight}cm (${desk.minHeight}-${desk.maxHeight}cm)
                        </small>
                    </p>
                    ${desk.isAssigned ? `
                        <p class="card-text mb-1">
                            <small class="text-muted">
                                Assigned to: ${desk.assignedUserName}
                                <br>Since: ${new Date(desk.assignmentStart).toLocaleString()}
                            </small>
                        </p>
                    ` : ''}
                </div>
                <div class="d-flex flex-column gap-2">
                    ${assignButton}
                    <div class="btn-group">
                        <button class="btn btn-sm btn-outline-secondary move-up-btn" data-desk-id="${desk.id}" title="Move desk up">
                            <i class="fas fa-chevron-up"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-secondary move-down-btn" data-desk-id="${desk.id}" title="Move desk down">
                            <i class="fas fa-chevron-down"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    return div;
}

// Refresh statistics
async function refreshStatistics(startDate = null, endDate = null) {
    console.log('Refreshing statistics');
    const selectedDeskElement = document.querySelector('.card[data-desk-id][data-selected-desk="true"]');
    const deskId = selectedDeskElement?.dataset.deskId;

    if (!deskId) {
        console.error('No desk selected');
        showAlert('Please select a desk first');
        return;
    }

    await loadDeskStatistics(deskId, startDate, endDate);
}

// Toggle chart view
function toggleChartView() {
    console.log('Toggling chart view');
    const statsSection = document.getElementById('statisticsSection');
    statsSection.classList.toggle('d-none');
}

// Load desk statistics
async function loadDeskStatistics(deskId, startDate = null, endDate = null) {
    console.log('Loading desk statistics for:', deskId);
    toggleLoading('usageChart', true);
    toggleLoading('heightChart', true);

    try {
        const endDateParam = endDate ? `&endDate=${endDate.toISOString()}` : '';
        const startDateParam = startDate ? `&startDate=${startDate.toISOString()}` : '';

        console.log('Sending desk statistics request');
        const response = await fetch(
            `?handler=DeskUsageStatistics&deskId=${deskId}${startDateParam}${endDateParam}`,
            {
                method: 'GET'
            }
        );

        console.log('Received response:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('Response data:', data);
        
        if (data.success) {
            updateUsageChart(data.statistics);
            updateHeightAdjustmentsChart(data.statistics);
        } else {
            console.error('Failed to load statistics:', data.error);
            showAlert(`Error loading statistics: ${data.error}`);
        }
    } catch (error) {
        console.error('Error loading statistics:', error);
        showAlert(`Failed to load statistics: ${error.message}`);
    } finally {
        toggleLoading('usageChart', false);
        toggleLoading('heightChart', false);
    }
}

// Update usage chart
function updateUsageChart(statistics) {
    console.log('Updating usage chart');
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
    console.log('Updating height adjustments chart');
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
    console.log('Selecting desk:', deskId);
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
    console.log('Performing search with query:', query);
    toggleLoading('deskList', true);

    try {
        console.log('Sending search request');
        const response = await fetch(`?handler=Search&searchQuery=${encodeURIComponent(query)}`, {
            method: 'POST'
        });

        console.log('Received response:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('Response data:', data);
        
        if (data.success) {
            updateDeskList(data.results, false);  // Pass false to prevent auto-selection
        } else {
            console.error('Failed to search desks:', data.error);
            showAlert(`Error searching desks: ${data.error}`);
        }
    } catch (error) {
        console.error('Error searching desks:', error);
        showAlert(`Failed to search desks: ${error.message}`);
    } finally {
        toggleLoading('deskList', false);
    }
}

// Update desk list
function updateDeskList(desks, autoSelect = true) {
    console.log('Updating desk list with:', desks);
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

// Load initial desks
async function loadInitialDesks() {
    console.log('Loading initial desks');
    try {
        console.log('Sending initial desks request');
        const response = await fetch('?handler=GetAllDesks', {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            }
        });

        console.log('Received response:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        console.log('Response data:', JSON.stringify(result, null, 2));
        
        if (result.success) {
            console.log('Successfully loaded desks:', result.desks);
            updateDeskLists(result.desks);
        } else {
            console.error('Failed to load desks:', result.error);
            showAlert('Failed to load desks', 'danger');
        }
    } catch (error) {
        console.error('Error loading desks:', error);
        showAlert('An error occurred while loading desks', 'danger');
    }
}
