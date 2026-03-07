import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import Header from '../common/Header.jsx';
import WorkshopDetailsForm from './WorkshopDetailsForm.jsx';
import { workshopApi } from '../../services/workshopApi.js';

const WorkshopDetailsInitial = () => {
    const navigate = useNavigate();
    const { login } = useAuth();

    const handleSubmit = async (e, formData) => {
        e.preventDefault();
        const response = await workshopApi.create(formData);
        
        // Update auth context with new accesses and user info from response
        if (response) {
            login(response);
        }
        
        localStorage.setItem('HasWorkshop', 'true');
        navigate('/');
    };

    return (
        <div className="vertical" style={{ height: '100vh' }}>
            <Header />
            <main className="main" style={{ display: 'flex', alignItems: 'center' }}>
                <div className="tile" style={{ width: 'fit-content', marginTop: '75px' }}>
                    <h3 className="tile-header">Workshop Information</h3>
                    <WorkshopDetailsForm handleSubmit={handleSubmit} />
                </div>
            </main>
        </div>
    );
};

export default WorkshopDetailsInitial;
