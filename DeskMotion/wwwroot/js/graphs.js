document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('canvas[data-chart-type]').forEach(canvas => {
        const type = canvas.dataset.chartType;
        const labels = JSON.parse(canvas.dataset.chartLabels || '[]');
        const data = JSON.parse(canvas.dataset.chartData || '[]');
        const labelText = canvas.dataset.chartLabelText || 'Data';
        const backgroundColors = canvas.dataset.chartBackgroundcolors ? JSON.parse(canvas.dataset.chartBackgroundcolors) : null;
        const borderColor = canvas.dataset.chartBordercolor || 'rgba(255, 99, 132, 1)';
        const backgroundColor = canvas.dataset.chartBackgroundcolor || 'rgba(255, 99, 132, 0.2)';
        const ctx = canvas.getContext('2d');
        let config;

        switch (type) {
            case 'bar':
                config = {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: labelText,
                            data: data,
                            backgroundColor,
                            borderColor,
                            borderWidth: 1
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            legend: { position: 'top' },
                            title: { display: true, text: labelText + ' Chart' }
                        }
                    }
                };
                break;
            case 'pie':
                config = {
                    type: 'pie',
                    data: {
                        labels: labels,
                        datasets: [{
                            data: data,
                            backgroundColor: backgroundColors || ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40']
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            legend: { position: 'top' },
                            title: { display: true, text: labelText + ' Chart' }
                        }
                    }
                };
                break;
            case 'line':
                config = {
                    type: 'line',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: labelText,
                            data: data,
                            backgroundColor: backgroundColor || 'rgba(75, 192, 192, 0.2)',
                            borderColor: borderColor || 'rgba(75, 192, 192, 1)',
                            borderWidth: 1
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            legend: { position: 'top' },
                            title: { display: true, text: labelText + ' Chart' }
                        }
                    }
                };
                break;
            default:
                console.warn('Unknown chart type:', type);
        }

        if (config) {
            _ = new Chart(ctx, config);
        }
    });
});
