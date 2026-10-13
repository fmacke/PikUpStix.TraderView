import tradeService from './tradeService';
import notesService from './notesService';
import riskService from './riskService';
import syncService from './syncService';
import positionsService from './positionsService';
import listsService from './listsService';

// Backwards-compatible facade that composes domain services.
// Consumers can migrate to importing domain services directly (e.g. import tradeService from './services/tradeService')
export const apiService = {
    ...tradeService,
    ...notesService,
    ...riskService,
    ...syncService,
    ...positionsService,
    ...listsService,
};

export default apiService;

