// API Configuration
const API_BASE_URL = window.location.origin;

// Global variables
let availableProducts = [];
let selectedProducts = {};

// Load products on page load
document.addEventListener('DOMContentLoaded', async () => {
    await loadProducts();
    addLog('System ready! Select products and click the button to begin.', 'success');
});

// Load available products from database
async function loadProducts() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/orders/products`);
        
        if (!response.ok) {
            throw new Error(`HTTP Error ${response.status}`);
        }
        
        availableProducts = await response.json();
        displayProducts();
        
    } catch (error) {
        console.error('Error loading products:', error);
        document.getElementById('productsLoadingMessage').innerHTML = `
            <p style="color: #dc3545;">Failed to load products. Please refresh the page.</p>
        `;
    }
}

// Display products in the UI
function displayProducts() {
    const loadingMsg = document.getElementById('productsLoadingMessage');
    const container = document.getElementById('productsContainer');
    
    loadingMsg.style.display = 'none';
    container.style.display = 'block';
    
    if (availableProducts.length === 0) {
        container.innerHTML = `
            <div class="no-products-message">
                <p>No products available in stock</p>
            </div>
        `;
        return;
    }
    
    container.innerHTML = availableProducts.map(product => `
        <div class="product-item" id="product-${product.name}" data-product="${product.name}">
            <div class="product-info">
                <div class="product-name">${product.name}</div>
                <div class="product-price">$${product.unitPrice.toFixed(2)}</div>
                <div class="product-stock ${product.stockQuantity < 5 ? 'low-stock' : ''}">
                    ${product.stockQuantity} in stock
                </div>
            </div>
            <div class="quantity-controls">
                <button type="button" class="quantity-btn" onclick="decreaseQuantity('${product.name}')">-</button>
                <input type="number" 
                       class="quantity-input" 
                       id="qty-${product.name}" 
                       value="0" 
                       min="0" 
                       max="${product.stockQuantity}"
                       onchange="updateQuantity('${product.name}', this.value)">
                <button type="button" class="quantity-btn" onclick="increaseQuantity('${product.name}')">+</button>
            </div>
            <div class="product-total" id="total-${product.name}">$0.00</div>
        </div>
    `).join('');
    
    // Initialize selected products
    availableProducts.forEach(product => {
        selectedProducts[product.name] = {
            quantity: 0,
            unitPrice: product.unitPrice,
            stockQuantity: product.stockQuantity
        };
    });
}

// Increase product quantity
function increaseQuantity(productName) {
    const input = document.getElementById(`qty-${productName}`);
    const max = parseInt(input.max);
    const current = parseInt(input.value);
    
    if (current < max) {
        input.value = current + 1;
        updateQuantity(productName, input.value);
    }
}

// Decrease product quantity
function decreaseQuantity(productName) {
    const input = document.getElementById(`qty-${productName}`);
    const current = parseInt(input.value);
    
    if (current > 0) {
        input.value = current - 1;
        updateQuantity(productName, input.value);
    }
}

// Update quantity and recalculate totals
function updateQuantity(productName, quantity) {
    quantity = parseInt(quantity) || 0;
    const product = selectedProducts[productName];
    
    if (!product) return;
    
    // Ensure quantity doesn't exceed stock
    if (quantity > product.stockQuantity) {
        quantity = product.stockQuantity;
        document.getElementById(`qty-${productName}`).value = quantity;
    }
    
    if (quantity < 0) {
        quantity = 0;
        document.getElementById(`qty-${productName}`).value = 0;
    }
    
    product.quantity = quantity;
    
    // Update product total
    const total = quantity * product.unitPrice;
    document.getElementById(`total-${productName}`).textContent = `$${total.toFixed(2)}`;
    
    // Highlight selected products
    const productElement = document.getElementById(`product-${productName}`);
    if (quantity > 0) {
        productElement.classList.add('selected');
    } else {
        productElement.classList.remove('selected');
    }
    
    // Update order total
    updateOrderTotal();
}

// Calculate and update order total
function updateOrderTotal() {
    let total = 0;
    
    Object.values(selectedProducts).forEach(product => {
        total += product.quantity * product.unitPrice;
    });
    
    document.getElementById('orderTotal').textContent = `$${total.toFixed(2)}`;
}

// Add log entry
function addLog(message, type = 'info') {
    const logsContent = document.getElementById('logsContent');
    const logEmpty = logsContent.querySelector('.log-empty');
    
    if (logEmpty) {
        logEmpty.remove();
    }
    
    const timestamp = new Date().toLocaleTimeString('en-US');
    const logEntry = document.createElement('div');
    logEntry.className = `log-entry log-${type}`;
    logEntry.innerHTML = `<span class="log-timestamp">[${timestamp}]</span> ${message}`;
    
    logsContent.appendChild(logEntry);
    logsContent.scrollTop = logsContent.scrollHeight;
}

// Clear logs
function clearLogs() {
    const logsContent = document.getElementById('logsContent');
    logsContent.innerHTML = '<p class="log-empty">Waiting for order processing...</p>';
    
    document.getElementById('orderResult').style.display = 'none';
    document.getElementById('invoiceResult').style.display = 'none';
    document.getElementById('packageResult').style.display = 'none';
    
    // Reset all product quantities to 0
    Object.keys(selectedProducts).forEach(productName => {
        selectedProducts[productName].quantity = 0;
        const input = document.getElementById(`qty-${productName}`);
        if (input) {
            input.value = 0;
        }
        const total = document.getElementById(`total-${productName}`);
        if (total) {
            total.textContent = '$0.00';
        }
        const productElement = document.getElementById(`product-${productName}`);
        if (productElement) {
            productElement.classList.remove('selected');
        }
    });
    
    // Reset order total
    document.getElementById('orderTotal').textContent = '$0.00';
    
    addLog('🗑️ Logs cleared and order reset', 'info');
}

// Display order details
function displayOrderDetails(order) {
    const orderDetails = document.getElementById('orderDetails');
    
    orderDetails.innerHTML = `
        <div class="detail-item">
            <span class="detail-label">Order Number:</span>
            <span class="detail-value">${order.orderNumber}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Customer:</span>
            <span class="detail-value">${order.customerName}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Email:</span>
            <span class="detail-value">${order.customerEmail}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Delivery Address:</span>
            <span class="detail-value">${order.deliveryAddress.street}, ${order.deliveryAddress.city}, ${order.deliveryAddress.postalCode}, ${order.deliveryAddress.country}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Total:</span>
            <span class="detail-value" style="font-size: 1.2em; font-weight: bold; color: #28a745;">$${order.totalAmount}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Status:</span>
            <span class="detail-value" style="color: #28a745; font-weight: bold;">${order.status}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Placed At:</span>
            <span class="detail-value">${new Date(order.placedAt).toLocaleString('en-US')}</span>
        </div>
    `;
    
    document.getElementById('orderResult').style.display = 'block';
}

// Display invoice details
function displayInvoiceDetails(invoice) {
    const invoiceDetails = document.getElementById('invoiceDetails');
    
    invoiceDetails.innerHTML = `
        <div class="detail-item">
            <span class="detail-label">Invoice Number:</span>
            <span class="detail-value">${invoice.invoiceNumber}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Associated Order:</span>
            <span class="detail-value">${invoice.orderNumber}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Customer:</span>
            <span class="detail-value">${invoice.customerName}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Subtotal:</span>
            <span class="detail-value">$${invoice.totalAmount}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">VAT (19%):</span>
            <span class="detail-value">$${invoice.vatAmount}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Total with VAT:</span>
            <span class="detail-value" style="font-size: 1.2em; font-weight: bold; color: #28a745;">$${invoice.totalWithVat}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Issue Date:</span>
            <span class="detail-value">${new Date(invoice.issueDate).toLocaleString('en-US')}</span>
        </div>
    `;
    
    document.getElementById('invoiceResult').style.display = 'block';
}

// Display package details
function displayPackageDetails(pkg) {
    const packageDetails = document.getElementById('packageDetails');
    
    packageDetails.innerHTML = `
        <div class="detail-item">
            <span class="detail-label">AWB (Tracking):</span>
            <span class="detail-value" style="font-family: monospace; font-size: 1.1em; font-weight: bold;">${pkg.awb}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Associated Order:</span>
            <span class="detail-value">${pkg.orderNumber}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Delivery Address:</span>
            <span class="detail-value">${pkg.deliveryAddress.street}, ${pkg.deliveryAddress.city}, ${pkg.deliveryAddress.postalCode}, ${pkg.deliveryAddress.country}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Courier Status:</span>
            <span class="detail-value" style="color: #28a745; font-weight: bold;">${pkg.courierNotificationStatus}</span>
        </div>
        <div class="detail-item">
            <span class="detail-label">Pickup Date:</span>
            <span class="detail-value">${new Date(pkg.pickupDate).toLocaleString('en-US')}</span>
        </div>
    `;
    
    document.getElementById('packageResult').style.display = 'block';
}

// Process order (Step 1 - Place Order)
async function processOrder(orderData) {
    addLog('STEP 1: Starting order processing...', 'info');
    addLog('Sending request to API /api/orders...', 'info');
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/orders`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(orderData)
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP Error ${response.status}: ${errorText}`);
        }
        
        const order = await response.json();
        
        addLog('Response received from server', 'success');
        addLog(`Order placed successfully! Number: ${order.orderNumber}`, 'success');
        addLog(`Order total: $${order.totalAmount}`, 'info');
        addLog(`Status: ${order.status}`, 'success');
        addLog('Event published: OrderPlaced', 'info');
        
        displayOrderDetails(order);
        
        // Continue with invoice generation
        await generateInvoice(order);
        
    } catch (error) {
        addLog(`ERROR placing order: ${error.message}`, 'error');
        console.error('Order error:', error);
        throw error;
    }
}

// Generate invoice (Step 2 - Billing)
async function generateInvoice(order) {
    addLog('', 'info');
    addLog('STEP 2: Starting invoice generation...', 'info');
    addLog('Waiting 2 seconds for event processing...', 'warning');
    
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    addLog('Sending request to API /api/invoices...', 'info');
    
    const invoiceData = {
        orderNumber: order.orderNumber,
        customerName: order.customerName,
        totalAmount: String(order.totalAmount)
    };
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/invoices`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(invoiceData)
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP Error ${response.status}: ${errorText}`);
        }
        
        const invoice = await response.json();
        
        addLog('Response received from server', 'success');
        addLog(`Invoice generated successfully! Number: ${invoice.invoiceNumber}`, 'success');
        addLog(`Subtotal: $${invoice.totalAmount}`, 'info');
        addLog(`VAT (19%): $${invoice.vatAmount}`, 'info');
        addLog(`Total with VAT: $${invoice.totalWithVat}`, 'success');
        addLog('Event published: InvoiceGenerated', 'info');
        addLog('', 'info');
        addLog('PAYMENT PROCESSED!', 'success');
        addLog(`Order ${order.orderNumber} marked as PAID`, 'success');
        addLog(`Payment date: ${new Date().toLocaleString('en-US')}`, 'info');
        
        displayInvoiceDetails(invoice);
        
        // Continue with shipping
        await shipPackage(order);
        
    } catch (error) {
        addLog(`ERROR generating invoice: ${error.message}`, 'error');
        console.error('Invoice error:', error);
        throw error;
    }
}

// Ship package (Step 3 - Shipping)
async function shipPackage(order) {
    addLog('', 'info');
    addLog('STEP 3: Starting shipping process...', 'info');
    addLog('Waiting 2 seconds for event processing...', 'warning');
    
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    addLog('Sending request to API /api/packages...', 'info');
    
    const packageData = {
        orderNumber: order.orderNumber,
        deliveryStreet: order.deliveryAddress.street,
        deliveryCity: order.deliveryAddress.city,
        deliveryPostalCode: order.deliveryAddress.postalCode,
        deliveryCountry: order.deliveryAddress.country
    };
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/packages`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(packageData)
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP Error ${response.status}: ${errorText}`);
        }
        
        const pkg = await response.json();
        
        addLog('Response received from server', 'success');
        addLog(`Package shipped successfully! AWB: ${pkg.awb}`, 'success');
        addLog(`Courier status: ${pkg.courierNotificationStatus}`, 'success');
        addLog('Event published: PackageShipped', 'info');
        addLog('', 'info');
        addLog('PROCESSING COMPLETE! All workflows executed successfully!', 'success');
        
        displayPackageDetails(pkg);
        
        // Reload products to update stock
        await loadProducts();
        addLog('♻️ Product stock updated', 'info');
        
    } catch (error) {
        addLog(`ERROR shipping package: ${error.message}`, 'error');
        console.error('Package error:', error);
        throw error;
    }
}

// Form submit handler
document.getElementById('orderForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const submitBtn = document.getElementById('submitBtn');
    const originalText = submitBtn.innerHTML;
    
    // Get selected products
    const items = Object.entries(selectedProducts)
        .filter(([_, product]) => product.quantity > 0)
        .map(([name, product]) => ({
            productName: name,
            quantity: String(product.quantity),
            unitPrice: String(product.unitPrice)
        }));
    
    if (items.length === 0) {
        alert('Please select at least one product!');
        return;
    }
    
    // Disable button
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner"></span> Processing...';
    
    // Clear previous results
    clearLogs();
    
    // Collect form data
    const formData = {
        customerName: document.getElementById('customerName').value,
        customerEmail: document.getElementById('customerEmail').value,
        deliveryStreet: document.getElementById('deliveryStreet').value,
        deliveryCity: document.getElementById('deliveryCity').value,
        deliveryPostalCode: document.getElementById('deliveryPostalCode').value,
        deliveryCountry: document.getElementById('deliveryCountry').value,
        items: items
    };
    
    addLog('Order data collected from form', 'info');
    addLog(`Customer: ${formData.customerName}`, 'info');
    addLog(`Email: ${formData.customerEmail}`, 'info');
    addLog(`Delivery: ${formData.deliveryCity}, ${formData.deliveryCountry}`, 'info');
    addLog(`Products: ${items.length}`, 'info');
    addLog('', 'info');
    
    try {
        await processOrder(formData);
    } catch (error) {
        addLog('', 'error');
        addLog('Processing stopped due to an error.', 'error');
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalText;
    }
});
