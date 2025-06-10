
CREATE TABLE IF NOT EXISTS People (
    person_id INT AUTO_INCREMENT PRIMARY KEY,
    secret_code VARCHAR(50) UNIQUE NOT NULL, 
    full_name VARCHAR(255) NULL, 
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    total_reports_submitted INT DEFAULT 0,
    average_report_length DECIMAL(10, 2) DEFAULT 0.00
);

CREATE TABLE IF NOT EXISTS Reports (
    report_id INT AUTO_INCREMENT PRIMARY KEY,
    reporter_id INT NOT NULL,
    target_id INT NOT NULL,
    report_text TEXT NOT NULL,
    submission_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (reporter_id) REFERENCES People(person_id),
    FOREIGN KEY (target_id) REFERENCES People(person_id)
);

CREATE TABLE IF NOT EXISTS Alerts (
    alert_id INT AUTO_INCREMENT PRIMARY KEY,
    target_id INT NOT NULL,
    alert_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    time_window_start DATETIME NULL, 
    time_window_end DATETIME NULL,  
    reason TEXT NOT NULL,            
    FOREIGN KEY (target_id) REFERENCES People(person_id)
);

CREATE TABLE IF NOT EXISTS AppLogs (
    log_id INT AUTO_INCREMENT PRIMARY KEY,
    log_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    log_level VARCHAR(20) NOT NULL,
    activity_type VARCHAR(50) NOT NULL, 
    description TEXT NOT NULL
);

CREATE INDEX idx_people_secret_code ON People(secret_code);
CREATE INDEX idx_people_full_name ON People(full_name);
CREATE INDEX idx_reports_reporter_id ON Reports(reporter_id);
CREATE INDEX idx_reports_target_id ON Reports(target_id);
CREATE INDEX idx_reports_submission_time ON Reports(submission_time);
CREATE INDEX idx_alerts_target_id ON Alerts(target_id);